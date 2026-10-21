using System.Net;
using System.Net.Http.Json;
using Application.DTOs.Auth;
using Application.DTOs.Common;
using FluentAssertions;
using Xunit;

namespace API.Tests;

public class AuthApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RegisterAndLogin_Flow_ShouldSucceed()
    {
        // 1. Register
        var registerRequest = new RegisterRequestDto
        {
            Username = "user_" + Guid.NewGuid().ToString("N")[..8],
            Email = "user_" + Guid.NewGuid().ToString("N")[..8] + "@crn.com",
            Password = "SecurePassword123!",
            Role = "User"
        };

        var registerResponse = await _client.PostAsJsonAsync("/api/v1/auth/register", registerRequest);
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var regEnvelope = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
        regEnvelope.Should().NotBeNull();
        regEnvelope!.Success.Should().BeTrue();
        regEnvelope.Data!.Token.Should().NotBeNullOrEmpty();
        regEnvelope.Data.RefreshToken.Should().NotBeNullOrEmpty();

        // 2. Login
        var loginRequest = new LoginRequestDto
        {
            Username = registerRequest.Username,
            Password = registerRequest.Password
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loginEnvelope = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
        loginEnvelope.Should().NotBeNull();
        loginEnvelope!.Success.Should().BeTrue();
        loginEnvelope.Data!.Token.Should().NotBeNullOrEmpty();
        loginEnvelope.Data.RefreshToken.Should().NotBeNullOrEmpty();

        // 3. Refresh Token Rotation
        var refreshRequest = new RefreshTokenRequestDto
        {
            Token = loginEnvelope.Data.Token,
            RefreshToken = loginEnvelope.Data.RefreshToken
        };

        var refreshResponse = await _client.PostAsJsonAsync("/api/v1/auth/refresh-token", refreshRequest);
        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshEnvelope = await refreshResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResponseDto>>();
        refreshEnvelope.Should().NotBeNull();
        refreshEnvelope!.Success.Should().BeTrue();
        refreshEnvelope.Data!.RefreshToken.Should().NotBe(loginEnvelope.Data.RefreshToken, "Token rotation generates a new token");
    }
}
