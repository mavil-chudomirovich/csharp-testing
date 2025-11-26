using Application;
using Application.Abstractions;
using Application.AppExceptions;
using Application.AppSettingConfigurations;
using Application.Constants;
using Application.Dtos.User.Request;
using Application.Helpers;
using Application.Repositories;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;

namespace GreenWheel.Tests.Application.Service
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<IRefreshTokenRepository> _refreshTokenRepoMock;
        private readonly Mock<IJwtBlackListRepository> _jwtBlackListRepoMock;
        private readonly Mock<IOTPRepository> _otpRepoMock;
        private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Mock<IMemoryCache> _cacheMock;
        private readonly Mock<IEmailSerivce> _emailServiceMock;
        private readonly Mock<ITokenService> _tokenServiceMock;

        private readonly IOptions<JwtSettings> _jwtOptions;
        private readonly IOptions<OTPSettings> _otpOptions;

        private readonly AuthSerivce _authService;

        public AuthServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
            _jwtBlackListRepoMock = new Mock<IJwtBlackListRepository>();
            _otpRepoMock = new Mock<IOTPRepository>();
            _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            _mapperMock = new Mock<IMapper>();
            _cacheMock = new Mock<IMemoryCache>();
            _emailServiceMock = new Mock<IEmailSerivce>();
            _tokenServiceMock = new Mock<ITokenService>();

            // fake JWT settings
            _jwtOptions = Options.Create(new JwtSettings
            {
                AccessTokenSecret = "secretsecretsecretsecret",
                RefreshTokenSecret = "refreshsecretrefreshsecret",
                AccessTokenExpiredTime = 15,
                RegisterTokenExpiredTime = 10080,
                Issuer = "TestIssuer",
                Audience = "TestAudience"
            });

            _otpOptions = Options.Create(new OTPSettings());

            _authService = new AuthSerivce(
                _userRepoMock.Object,
                _jwtOptions,
                _refreshTokenRepoMock.Object,
                _jwtBlackListRepoMock.Object,
                _otpRepoMock.Object,
                _httpContextAccessorMock.Object,
                _otpOptions,
                _mapperMock.Object,
                _cacheMock.Object,
                _emailServiceMock.Object,
                _tokenServiceMock.Object
            );
        }

        // ================================
        // 1. LOGIN SUCCESS
        // ================================
        [Fact]
        public async Task Login_ShouldReturnAccessToken_WhenCredentialsAreValid()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var req = new UserLoginReq { Email = "test@mail.com", Password = "123456" };

            var testUser = new User
            {
                Id = userId,
                Email = req.Email,
                Password = PasswordHelper.HashPassword("123456"),
                IsGoogleLinked = false
            };

            _userRepoMock
                .Setup(r => r.GetByEmailAsync(req.Email))
                .ReturnsAsync(testUser);

            _tokenServiceMock
                .Setup(t => t.GenerateAccessToken(userId))
                .Returns("FAKE_ACCESS_TOKEN");

            _tokenServiceMock
                .Setup(t => t.GenerateRefreshToken(userId, null))
                .ReturnsAsync("FAKE_REFRESH_TOKEN");

            // Act
            var result = await _authService.Login(req);

            // Assert
            Assert.Equal("FAKE_ACCESS_TOKEN", result);
            _tokenServiceMock.Verify(t => t.GenerateRefreshToken(userId, null), Times.Once);
            _tokenServiceMock.Verify(t => t.GenerateAccessToken(userId), Times.Once);
        }
        // ================================
        // 2. GOOGLE ACCOUNT BUT NO PASSWORD
        // ================================
        [Fact]
        public async Task Login_ShouldThrowForbidden_WhenGoogleLinkedAndNoPassword()
        {
            var req = new UserLoginReq { Email = "g@mail.com", Password = "123" };

            var user = new User
            {
                Email = req.Email,
                IsGoogleLinked = true,
                Password = null
            };

            _userRepoMock.Setup(r => r.GetByEmailAsync(req.Email))
                .ReturnsAsync(user);

            var ex = await Assert.ThrowsAsync<ForbidenException>(() => _authService.Login(req));
            Assert.Equal(Message.UserMessage.NotHavePassword, ex.Message);
        }

        // ================================
        // 3. WRONG PASSWORD
        // ================================
        //[Fact]
        //public async Task Login_ShouldThrowUnauthorized_WhenPasswordWrong()
        //{
        //    var req = new UserLoginReq { Email = "lehoang@gmail.com", Password = "wrong" };

        //    var user = new User
        //    {
        //        Email = req.Email,
        //        IsGoogleLinked = false,
        //        Password = PasswordHelper.HashPassword("correct")
        //    };

        //    _userRepoMock
        //        .Setup(r => r.GetByEmailAsync(req.Email)).ReturnsAsync(user);
        //    var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.Login(req));
        //    Assert.Equal(Message.UserMessage.InvalidEmailOrPassword, ex.Message);
        //}

        //[Fact]
        //public async Task Login_ShouldThrowUnauthorized_WhenDBEmptyPassword()
        //{
        //    var req = new UserLoginReq { Email = "lehoang@gmail.com", Password = "wrong" };

        //    var user = new User
        //    {
        //        Email = req.Email,
        //        IsGoogleLinked = false,
        //        Password = null
        //    };

        //    _userRepoMock
        //        .Setup(r => r.GetByEmailAsync(req.Email)).ReturnsAsync(user);
        //    var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.Login(req));
        //    Assert.Equal(Message.UserMessage.InvalidEmailOrPassword, ex.Message);
        //}

        // ================================
        // 4. EMAIL NOT FOUND
        // ================================
        //[Fact]
        //public async Task Login_ShouldThrowUnauthorized_WhenEmailNotFound()
        //{
        //    var req = new UserLoginReq { Email = "a@gmail.com", Password = "notFound" };
            
        //    _userRepoMock.Setup(r => r.GetByEmailAsync(req.Email)).ReturnsAsync((User?)null);

        //    var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.Login(req));
        //    Assert.Equal(Message.UserMessage.InvalidEmailOrPassword, ex.Message);
        //}


        //[Theory]
        //[InlineData("lehoang@gmail.com", "wrong", false, "correct")] // wrong password
        //[InlineData("notfound@gmail.com", "123", false, null)]       // user not found
        //[InlineData("gg@gmail.com", "123", true, null)]        // google linked no password
        //public async Task Login_ShouldThrowUnauthorizedOrForbidden(
        //string email,
        //string passwordInput,
        //bool isGoogleLinked,
        //string? storedPassword)
        //{
        //    // Arrange
        //    var req = new UserLoginReq { Email = email, Password = passwordInput };

        //    //nếu không có password trong db và không phải google linked thì user = null
        //    User? user = storedPassword == null && !isGoogleLinked
        //        ? null
        //        : new User
        //        {
        //            Email = email,
        //            Password = storedPassword == null ? null : PasswordHelper.HashPassword(storedPassword),
        //            IsGoogleLinked = isGoogleLinked
        //        };

        //    _userRepoMock.Setup(r => r.GetByEmailAsync(email)).ReturnsAsync(user);

        //    // Act + Assert
        //    if (isGoogleLinked && storedPassword == null)
        //    {
        //        var ex = await Assert.ThrowsAsync<ForbidenException>(() => _authService.Login(req));
        //        Assert.Equal(Message.UserMessage.NotHavePassword, ex.Message);
        //    }
        //    else
        //    {
        //        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _authService.Login(req));
        //        Assert.Equal(Message.UserMessage.InvalidEmailOrPassword, ex.Message);
        //    }
        //}

    }
}