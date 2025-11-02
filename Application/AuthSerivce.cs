using Application.Abstractions;
using Application.AppExceptions;
using Application.AppSettingConfigurations;
using Application.Constants;
using Application.Dtos.User.Request;
using Application.Helpers;
using Application.Repositories;
using AutoMapper;
using Domain.Entities;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Application
{
    public class AuthSerivce : IAuthService
    {

        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly IEmailSerivce _emailService;
        private readonly JwtSettings _jwtSettings;
        private readonly OTPSettings _otpSettings;
        private readonly IOTPRepository _otpRepository;
        private readonly IJwtBlackListRepository _jwtBackListRepository;

        public AuthSerivce(IUserRepository repository,
            IOptions<JwtSettings> jwtSettings,
            IRefreshTokenRepository refreshTokenRepository,
             IJwtBlackListRepository jwtBackListRepository,
             IOTPRepository otpRepository,
             IHttpContextAccessor httpContextAccessor,
             IOptions<OTPSettings> otpSetting,
             IMapper mapper,
             IMemoryCache cache,
             IEmailSerivce emailSerivce
            )
        {
            _userRepository = repository;
            _refreshTokenRepository = refreshTokenRepository;
            _otpRepository = otpRepository;
            _jwtBackListRepository = jwtBackListRepository;
            _emailService = emailSerivce;
            _jwtSettings = jwtSettings.Value;
            _contextAccessor = httpContextAccessor;
            _otpSettings = otpSetting.Value;
            _mapper = mapper;
            _cache = cache;
        }
        public async Task<string?> Login(UserLoginReq user)
        {
            User? userFromDB = await _userRepository.GetByEmailAsync(user.Email);

            if (userFromDB != null)
            {
                if (userFromDB.IsGoogleLinked && userFromDB.Password == null)
                {
                    throw new ForbidenException(Message.UserMessage.NotHavePassword);
                }
                //if (PasswordHelper.VerifyPassword(user.Password, userFromDB.Password))
                if (userFromDB.Password != null && PasswordHelper.VerifyPassword(user.Password, userFromDB.Password))
                {
                    //tạo refreshtoken và lưu nó vào DB lẫn cookie
                    await GenerateRefreshToken(userFromDB.Id, null);
                    return GenerateAccessToken(userFromDB.Id);
                }
            }
            throw new UnauthorizedAccessException(Message.UserMessage.InvalidEmailOrPassword);
        }

        /*
         Generate Access Token Func
        this func recieve userId
        use jwtHelper and give userID, accesstoken secret secret, type: accesstoken, access token expired time, isser and audience
        to generate access token
         */

        public string GenerateAccessToken(Guid userId)
        {
            return JwtHelper.GenerateUserIDToken(userId, _jwtSettings.AccessTokenSecret, TokenType.AccessToken.ToString(), _jwtSettings.AccessTokenExpiredTime, _jwtSettings.Issuer, _jwtSettings.Audience, null);
        }

        /*
         Generate Refresh Token Func
         This func recieve userId and a ClaimsPrincipal if any
            - When we use refresh token to got a new access token, we will generate a new refresh
              token with expired time of old refresh token if it was not expired
              so that we will give a ClaimsPricipal for that func
        It use jwt helper to generate a token
        then verify this token to got a claimPricipal to take Iat (created time), Exp (expired time) to save this toke to DB

        Then set this token to cookie
         */

        public async Task<string> GenerateRefreshToken(Guid userId, ClaimsPrincipal? oldClaims)
        {
            var _context = _contextAccessor.HttpContext;
            string token = JwtHelper.GenerateUserIDToken(userId, _jwtSettings.RefreshTokenSecret, TokenType.RefreshToken.ToString(),
                _jwtSettings.RefreshTokenExpiredTime, _jwtSettings.Issuer, _jwtSettings.Audience, oldClaims);
            ClaimsPrincipal claims = JwtHelper.VerifyToken(token, _jwtSettings.RefreshTokenSecret, TokenType.RefreshToken.ToString(),
                _jwtSettings.Issuer, _jwtSettings.Audience);
            long.TryParse(claims.FindFirst(JwtRegisteredClaimNames.Iat)?.Value, out long iatSeconds);
            long.TryParse(claims.FindFirst(JwtRegisteredClaimNames.Exp)?.Value, out long expSeconds);

            await _refreshTokenRepository.AddAsync(new RefreshToken()
            {
                UserId = userId,
                Token = token,
                IssuedAt = DateTimeOffset.FromUnixTimeSeconds(iatSeconds).UtcDateTime,
                CreatedAt = DateTimeOffset.FromUnixTimeSeconds(iatSeconds).UtcDateTime,
                UpdatedAt = DateTimeOffset.FromUnixTimeSeconds(iatSeconds).UtcDateTime,
                ExpiresAt = DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime,
                IsRevoked = false
            });
            //lưu vào cookie
            if(_context != null)
            {
                _context.Response.Cookies.Append(CookieKeys.RefreshToken, token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,    //BẮT BUỘC CHO CROSS-DOMAIN
                    Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.RefreshTokenExpiredTime), // hạn sử dụng
                    //Domain = ".greenwheel.site",     //để FE gửi cookie vào BE
                    Path = "/"
                });
            }
            return token;
        }

        /*
         Send OTP Func
        This function recieve email from controller
        User can got 1 Otp per minutes
        if it func call > 1 turn per minutes -> throw Rate Limit Exceeded Exception (429)
        Remove old OTP in DB before save new Otp
        generate new otp -> save to DB
        send this otp to user email
         */

        public async Task SendOTP(string email)
        {
            int count = await _otpRepository.CountRateLimitAsync(email);
            if (count > _otpSettings.OtpRateLimit)
            {
                throw new RateLimitExceededException(Message.UserMessage.RateLimitOtp);
            }
            await _otpRepository.RemoveOTPAsync(email); //xoá cũ trước khi lưu cái ms
            string otp = GenerateOtpHelper.GenerateOtp();
            await _otpRepository.SaveOTPAsyns(email, otp);
            string subject = "GreenWheel Verification Code";
            var basePath = AppContext.BaseDirectory;
            var templatePath = Path.Combine(basePath, "Templates", "SendOtpTemplate.html");
            var body = File.ReadAllText(templatePath);

            body = body.Replace("{OtpCode}", otp);
            await _emailService.SendEmailAsync(email, subject, body);
        }

        /*
         Verify OTP function
         this function recieve verifyOTPDto from controller include OTP and email
            Token type (type of token to generate) and cookieKey (name of token to save to cookie

        First we use email to take otp from DB
            - Null => this email do not have OTP -> throw Unauthorize Exception (401)
            - !null => this email got a OTP

        Next check OTP & OTP form DB
            - if != => count number of times entered & throw Unauthorize Exception (401)
                       if count > number of entries allowed -> delete otp form DB

        Then generate token by email belong to token type and set it to cookie
            - Register token when register account
            - forgot password token when user forgot thier password
        */

        public async Task<string> VerifyOTP(VerifyOTPReq verifyOTPDto, TokenType type, string cookieKey)
        {
            string? otpFromRedis = await _otpRepository.GetOtpAsync(verifyOTPDto.Email);
            if (otpFromRedis == null)
            {
                throw new UnauthorizedAccessException(Message.UserMessage.InvalidOTP);
            }
            if (verifyOTPDto.OTP != otpFromRedis)
            {
                int count = await _otpRepository.CountAttemptAsync(verifyOTPDto.Email);
                if (count > _otpSettings.OtpAttempts)
                {
                    await _otpRepository.RemoveOTPAsync(verifyOTPDto.Email);
                    await _otpRepository.ResetAttemptAsync(verifyOTPDto.Email);
                    throw new UnauthorizedAccessException(Message.CommonMessage.TooManyRequest);
                }
                throw new UnauthorizedAccessException(Message.UserMessage.InvalidOTP);
            }
            var _context = _contextAccessor.HttpContext;
            await _otpRepository.RemoveOTPAsync(verifyOTPDto.Email);
            string secret = "";
            int expiredTime;
            if (type == TokenType.RegisterToken)
            {
                secret = _jwtSettings.RegisterTokenSecret;
                expiredTime = _jwtSettings.RegisterTokenExpiredTime;
            }
            else
            {
                secret = _jwtSettings.ForgotPasswordTokenSecret;
                expiredTime = _jwtSettings.ForgotPasswordTokenExpiredTime;
            }
            string token = JwtHelper.GenerateEmailToken(verifyOTPDto.Email, secret, type.ToString(), expiredTime, _jwtSettings.Issuer, _jwtSettings.Audience, null);
            if (_context != null)
            {
                _context.Response.Cookies.Append(cookieKey, token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,    //BẮT BUỘC CHO CROSS-DOMAIN
                    Expires = DateTime.UtcNow.AddMinutes(expiredTime),
                    //Domain = ".greenwheel.site",     //để FE gửi cookie vào BE
                    Path = "/"
                });
            }
            return token;
        }

        /*
         Register account func
         This function receive token from controller ( register token ) => done verify otp And userRegisterReq
                                                                                            include user info
         it map userRegisterReq to user
         Then verify this token
            -   401 : invalid token
            - If success -> got claimsPricipal of this token
         got email form this claimPrincipal
         try got user form Db by email
            - != null -> throw Duplicate Exception (409)
            - == null -> create new account =>  generate refreshToken (set to cookie) and accessToken return to frontend
         */

        public async Task<string> RegisterAsync(string token, UserRegisterReq userRegisterReq)
        {
            if (await _userRepository.GetByPhoneAsync(userRegisterReq.Phone) != null)
            {
                throw new ConflictDuplicateException(Message.UserMessage.PhoneAlreadyExist);
            }
            //----check in black list
            if (await _jwtBackListRepository.CheckTokenInBlackList(token))
            {
                throw new UnauthorizedAccessException(Message.UserMessage.Unauthorized);
            }
            //------------------------
            var user = _mapper.Map<User>(userRegisterReq); //map từ một RegisterUserDto sang user
            var claims = JwtHelper.VerifyToken(token, _jwtSettings.RegisterTokenSecret,
                TokenType.RegisterToken.ToString(), _jwtSettings.Issuer, _jwtSettings.Audience);

            //var email = claims.FindFirst(JwtRegisteredClaimNames.Sid).Value.ToString();
            var sidClaim = claims.FindFirst(JwtRegisteredClaimNames.Sid);
            if (sidClaim == null || string.IsNullOrEmpty(sidClaim.Value))
            {
                throw new UnauthorizedAccessException(Message.UserMessage.Unauthorized);
            }
            var email = sidClaim.Value;
            var userFromDB = await _userRepository.GetByEmailAsync(email);

            if (userFromDB != null)
            {
                throw new ConflictDuplicateException(Message.UserMessage.EmailAlreadyExists); //email đã tồn tại
            }
            Guid id;
            do
            {
                id = Guid.NewGuid();
            } while (await _userRepository.GetByIdAsync(id) != null);
            //lấy ra list role trong cache
            var roles = _cache.Get<List<Role>>(Common.SystemCache.AllRoles);

            user.Id = id;
            user.CreatedAt = user.UpdatedAt = DateTime.UtcNow;
            user.Email = email;
            //user.RoleId = roles.FirstOrDefault(r => r.Name == RoleName.Customer).Id;
            var customerRole = roles?.FirstOrDefault(r => r.Name == RoleName.Customer);
            if (customerRole == null)
            {
                throw new InvalidOperationException("Customer role not found in roles cache.");
            }
            user.RoleId = customerRole.Id;
            user.DeletedAt = null;
            Guid userId = await _userRepository.AddAsync(user);
            string accesstoken = GenerateAccessToken(userId);
            string refreshToken = await GenerateRefreshToken(userId, null);

            //----save to black list
            //long.TryParse(claims.FindFirst(JwtRegisteredClaimNames.Exp).Value, out long expSeconds);
            long expSeconds = 0;
            var expClaim = claims.FindFirst(JwtRegisteredClaimNames.Exp);
            if (expClaim != null && !string.IsNullOrEmpty(expClaim.Value))
            {
                long.TryParse(expClaim.Value, out expSeconds);
            }
            await _jwtBackListRepository.SaveTokenAsyns(token, expSeconds);

            return accesstoken;
        }

        /*
         Change password func
         This func use for change password use case
         IT recieve userClaims from token of accessToken, oldPassword and new password => verify => take user ID from claims
         got user from DB by id
            - null -> throw unauthorized exception (401) (invalid accesstoken)
            - != null  -> verify password in DB == old password ?
                - == -> set new passwrd
                - != return unauthorized (401) (old password is incorrect)

         */

        public async Task ChangePassword(ClaimsPrincipal userClaims, UserChangePasswordReq userChangePasswordReq)
        {
            var userID = userClaims.FindFirstValue(JwtRegisteredClaimNames.Sid)!.ToString();
            var userFromDB = await _userRepository.GetByIdAsync(Guid.Parse(userID));
            if (userFromDB == null)
            {
                throw new UnauthorizedAccessException(Message.UserMessage.Unauthorized);
            }
            //if (userFromDB.Password != null && !PasswordHelper.VerifyPassword(userChangePasswordReq.OldPassword, userFromDB.Password))
            if (userFromDB.Password != null && 
                !string.IsNullOrEmpty(userChangePasswordReq.OldPassword) && 
                !PasswordHelper.VerifyPassword(userChangePasswordReq.OldPassword, userFromDB.Password))
            {
                throw new UnauthorizedAccessException(Message.UserMessage.OldPasswordIsIncorrect);
            }
            if (userFromDB.Password == null && !userFromDB.IsGoogleLinked)
            {
                throw new UnauthorizedAccessException(Message.UserMessage.OldPasswordIsIncorrect);
            }
            await _refreshTokenRepository.RevokeRefreshTokenByUserID(userID);
            userFromDB.Password = PasswordHelper.HashPassword(userChangePasswordReq.Password);
            await _userRepository.UpdateAsync(userFromDB);
        }

        /*
         Reset Password Func
         This function use for forgot password use case
         it recieve forgotPasswordToken (after verify email) and password from Controller
         verify this token
            - 401 : Invalid token

         if success -> got a claims -> take email form claim -> find user in DB by email
            - == null -> throw unAuthorized exception (401) : invalid token (hacker)
            - != null => revoke all refresh token of this account from DB and change password

         */

        public async Task ResetPassword(string forgotPasswordToken, string password)
        {
            //----check in black list
            if (await _jwtBackListRepository.CheckTokenInBlackList(forgotPasswordToken))
            {
                throw new UnauthorizedAccessException(Message.UserMessage.Unauthorized);
            }
            //------------------------
            var claims = JwtHelper.VerifyToken(forgotPasswordToken, _jwtSettings.ForgotPasswordTokenSecret,
                                                TokenType.ForgotPasswordToken.ToString(), _jwtSettings.Issuer, _jwtSettings.Audience);

            //------------------------
            string email = claims.FindFirstValue(JwtRegisteredClaimNames.Sid)!.ToString();
            var userFromDB = await _userRepository.GetByEmailAsync(email);
            if (userFromDB == null)
            {
                throw new UnauthorizedAccessException(Message.UserMessage.Unauthorized);
            }
            await _refreshTokenRepository.RevokeRefreshTokenByUserID(userFromDB.Id.ToString());
            userFromDB.Password = PasswordHelper.HashPassword(password);
            await _userRepository.UpdateAsync(userFromDB);
            //---- save to black list
            //long.TryParse(claims.FindFirst(JwtRegisteredClaimNames.Exp).Value, out long expSeconds);
            long expSeconds = 0;
            var expClaim = claims.FindFirst(JwtRegisteredClaimNames.Exp);
            if (expClaim != null && !string.IsNullOrEmpty(expClaim.Value))
            {
                long.TryParse(expClaim.Value, out expSeconds);
            }
            await _jwtBackListRepository.SaveTokenAsyns(forgotPasswordToken, expSeconds);
        }

        /*
         Logout func
         this function got refresh token from controller (cookie)
         -> revoke this token
         */

        public async Task<int> Logout(string refreshToken)
        {
            JwtHelper.VerifyToken(refreshToken, _jwtSettings.RefreshTokenSecret, TokenType.RefreshToken.ToString(), _jwtSettings.Issuer, _jwtSettings.Audience);
            return await _refreshTokenRepository.RevokeRefreshToken(refreshToken);
        }

        /*
         Refresh token func
         this function use to got new accesstoken by refresh token
         it receive refreshToken from controller, and a bool variable (want to be got a revoked token)
         verify this token
            - 401 : invalid token
         if success got a claim, got it token form BD by token
            - == null => 401 exception
           - != null -> generate new access token and refresh token with expired time = old refresh token expired time (use old claims)
         */

        public async Task<string> RefreshToken(string refreshToken, bool getRevoked)
        {
            ClaimsPrincipal claims = JwtHelper.VerifyToken(refreshToken,
                                                            _jwtSettings.RefreshTokenSecret,
                                                            TokenType.RefreshToken.ToString(),
                                                            _jwtSettings.Issuer,
                                                            _jwtSettings.Audience);

            if (claims != null)
            {
                RefreshToken? refreshTokenFromDB = await _refreshTokenRepository.GetByRefreshToken(refreshToken, getRevoked);
                if (refreshTokenFromDB == null)
                {
                    throw new UnauthorizedAccessException(Message.UserMessage.InvalidRefreshToken);
                }
                string newAccessToken = GenerateAccessToken(refreshTokenFromDB.UserId);
                string newRefreshToken = await GenerateRefreshToken(refreshTokenFromDB.UserId, claims);
                await _refreshTokenRepository.RevokeRefreshToken(refreshTokenFromDB.Token);

                return newAccessToken;
            }

            throw new UnauthorizedAccessException(Message.UserMessage.InvalidRefreshToken);
        }

        public async Task<Dictionary<string, string>> LoginWithGoogle(GoogleJsonWebSignature.Payload req)
        {
            var _context = _contextAccessor.HttpContext;
            User? user = await _userRepository.GetByEmailAsync(req.Email);
            if (user == null)
            {
                Guid id;
                do
                {
                    id = Guid.NewGuid();
                } while (await _userRepository.GetByIdAsync(id) != null);
                var roles = _cache.Get<List<Role>>(Common.SystemCache.AllRoles);
                if (roles == null)
                {
                    throw new InvalidOperationException("Roles cache is not initialized.");
                }
                user = new User
                {
                    Id = id,
                    FirstName = req.GivenName,
                    LastName = req.FamilyName,
                    AvatarUrl = req.Picture,
                    Email = req.Email,
                    Password = null,
                    DateOfBirth = null,
                    RoleId = roles.FirstOrDefault(r => r.Name == RoleName.Customer)!.Id,
                    IsGoogleLinked = true,
                    DeletedAt = null,
                };
                await _userRepository.AddAsync(user);
            }

            if (user.IsGoogleLinked == false) user.IsGoogleLinked = true;
            string accessToken = GenerateAccessToken(user.Id);
            await GenerateRefreshToken(user.Id, null);
            return new Dictionary<string, string>
            {
                { TokenType.AccessToken.ToString() , accessToken}
            };
        }
    }
}
