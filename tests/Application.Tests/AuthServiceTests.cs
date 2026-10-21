using Application.DTOs.Auth;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IPasswordHasher> _hasherMock;
    private readonly Mock<IJwtTokenGenerator> _tokenGenMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _userRepoMock = new Mock<IUserRepository>();
        _hasherMock = new Mock<IPasswordHasher>();
        _tokenGenMock = new Mock<IJwtTokenGenerator>();
        _loggerMock = new Mock<ILogger<AuthService>>();

        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepoMock.Object);

        _sut = new AuthService(
            _unitOfWorkMock.Object,
            _hasherMock.Object,
            _tokenGenMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task RegisterAsync_NewUser_ReturnsAuthResponse()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Username = "rushikesh",
            Email = "rushi@example.com",
            Password = "Password123!",
            Role = "User"
        };

        _userRepoMock.Setup(r => r.ExistsByUsernameOrEmailAsync("rushikesh", "rushi@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _hasherMock.Setup(h => h.HashPassword("Password123!")).Returns("hashed_pwd");
        _tokenGenMock.Setup(t => t.GenerateRefreshToken())
            .Returns(new RefreshToken { Token = "ref_123", ExpiresOn = DateTime.UtcNow.AddDays(7) });
        _tokenGenMock.Setup(t => t.GenerateAccessToken(It.IsAny<User>()))
            .Returns(("access_token_abc", DateTime.UtcNow.AddMinutes(30)));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _sut.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("access_token_abc");
        result.RefreshToken.Should().Be("ref_123");
        result.Username.Should().Be("rushikesh");
        _userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsTokens()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "john",
            PasswordHash = "hashed_pw"
        };

        _userRepoMock.Setup(r => r.GetByUsernameAsync("john", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasherMock.Setup(h => h.VerifyPassword("secret", "hashed_pw"))
            .Returns(true);
        _tokenGenMock.Setup(t => t.GenerateRefreshToken())
            .Returns(new RefreshToken { Token = "new_refresh_token" });
        _tokenGenMock.Setup(t => t.GenerateAccessToken(user))
            .Returns(("jwt_access_token", DateTime.UtcNow.AddMinutes(30)));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _sut.LoginAsync(new LoginRequestDto { Username = "john", Password = "secret" });

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("jwt_access_token");
        result.RefreshToken.Should().Be("new_refresh_token");
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ThrowsUnauthorizedException()
    {
        // Arrange
        var user = new User { Id = 1, Username = "john", PasswordHash = "hashed_pw" };
        _userRepoMock.Setup(r => r.GetByUsernameAsync("john", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _hasherMock.Setup(h => h.VerifyPassword("wrong", "hashed_pw"))
            .Returns(false);

        // Act
        Func<Task> act = async () => await _sut.LoginAsync(new LoginRequestDto { Username = "john", Password = "wrong" });

        // Assert
        await act.Should().ThrowAsync<UnauthorizedException>();
    }
}
