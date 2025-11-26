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
        private readonly IJwtBlackListRepository _jwtBlackListRepository;
        private readonly ITokenService _tokenService;

        public AuthSerivce(IUserRepository repository,
            IOptions<JwtSettings> jwtSettings,
            IRefreshTokenRepository refreshTokenRepository,
             IJwtBlackListRepository jwtBackListRepository,
             IOTPRepository otpRepository,
             IHttpContextAccessor httpContextAccessor,
             IOptions<OTPSettings> otpSetting,
             IMapper mapper,
             IMemoryCache cache,
             IEmailSerivce emailSerivce,
             ITokenService tokenService
            )
        {
            _userRepository = repository;
            _refreshTokenRepository = refreshTokenRepository;
            _otpRepository = otpRepository;
            _jwtBlackListRepository = jwtBackListRepository;
            _emailService = emailSerivce;
            _jwtSettings = jwtSettings.Value;
            _contextAccessor = httpContextAccessor;
            _otpSettings = otpSetting.Value;
            _mapper = mapper;
            _cache = cache;
            _tokenService = tokenService;
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
                    await _tokenService.GenerateRefreshToken(userFromDB.Id, null);
                    return _tokenService.GenerateAccessToken(userFromDB.Id);
                }
            }
            throw new UnauthorizedAccessException(Message.UserMessage.InvalidEmailOrPassword);
        }
    }
}
