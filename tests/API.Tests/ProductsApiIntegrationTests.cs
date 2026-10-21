using System.Net;
using System.Net.Http.Json;
using Application.DTOs.Common;
using Application.DTOs.Products;
using FluentAssertions;
using Xunit;

namespace API.Tests;

public class ProductsApiIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductsApiIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ReturnsSuccessAndPagedEnvelope()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/products?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResultDto<ProductDto>>>();
        envelope.Should().NotBeNull();
        envelope!.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task Create_ValidProduct_ReturnsCreatedWithEnvelope()
    {
        // Arrange
        var request = new CreateProductDto
        {
            ProductName = "Integration Test Laptop",
            CreatedBy = "CI Pipeline",
            Items = new List<CreateItemDto>
            {
                new() { Quantity = 3 }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        envelope.Should().NotBeNull();
        envelope!.Success.Should().BeTrue();
        envelope.Data!.ProductName.Should().Be("Integration Test Laptop");
        envelope.Data.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task Create_EmptyProductName_ReturnsBadRequestWithValidationErrors()
    {
        // Arrange
        var request = new CreateProductDto
        {
            ProductName = "",
            CreatedBy = "CI Pipeline"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
        envelope.Should().NotBeNull();
        envelope!.Success.Should().BeFalse();
        envelope.Errors.Should().NotBeNull();
    }
}
