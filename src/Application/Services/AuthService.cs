using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Users.ExistsByUsernameOrEmailAsync(request.Username, request.Email, cancellationToken))
        {
            throw new ValidationException("UsernameOrEmail", "A user with the specified username or email already exists.");
        }

        var role = Enum.TryParse<UserRole>(request.Role, true, out var parsedRole)
            ? parsedRole
            : UserRole.User;

        var user = new User
        {
            Username = request.Username.Trim(),
            Email = request.Email.Trim().ToLowerInvariant(),
            PasswordHash = _passwordHasher.HashPassword(request.Password),
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        user.RefreshTokens.Add(refreshToken);

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var (accessToken, expiresAt) = _jwtTokenGenerator.GenerateAccessToken(user);

        _logger.LogInformation("Registered user {Username} with role {Role}", user.Username, user.Role);

        return new AuthResponseDto
        {
            Token = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = expiresAt,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByUsernameAsync(request.Username.Trim(), cancellationToken);
        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid username or password.");
        }

        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        user.RefreshTokens.Add(refreshToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var (accessToken, expiresAt) = _jwtTokenGenerator.GenerateAccessToken(user);

        _logger.LogInformation("User {Username} logged in successfully", user.Username);

        return new AuthResponseDto
        {
            Token = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = expiresAt,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByRefreshTokenAsync(request.RefreshToken, cancellationToken);
        if (user == null)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        var existingToken = user.RefreshTokens.SingleOrDefault(r => r.Token == request.RefreshToken);
        if (existingToken == null || !existingToken.IsActive)
        {
            throw new UnauthorizedException("Refresh token is expired or revoked.");
        }

        // Token Rotation: revoke existing and issue new
        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();
        existingToken.RevokedOn = DateTime.UtcNow;
        existingToken.ReplacedByToken = newRefreshToken.Token;

        user.RefreshTokens.Add(newRefreshToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var (accessToken, expiresAt) = _jwtTokenGenerator.GenerateAccessToken(user);

        _logger.LogInformation("Refreshed token for user {Username} with rotation", user.Username);

        return new AuthResponseDto
        {
            Token = accessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = expiresAt,
            Username = user.Username,
            Email = user.Email,
            Role = user.Role.ToString()
        };
    }

    public async Task RevokeTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByRefreshTokenAsync(token, cancellationToken);
        if (user == null)
        {
            throw new NotFoundException("Token not found.");
        }

        var existingToken = user.RefreshTokens.SingleOrDefault(r => r.Token == token);
        if (existingToken == null || !existingToken.IsActive)
        {
            throw new ValidationException("Token", "Token is already revoked or inactive.");
        }

        existingToken.RevokedOn = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Revoked refresh token for user {Username}", user.Username);
    }
}
