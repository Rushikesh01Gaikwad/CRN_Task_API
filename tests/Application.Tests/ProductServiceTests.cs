using Application.DTOs.Products;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.Tests;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<ILogger<ProductService>> _loggerMock;
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepoMock = new Mock<IProductRepository>();
        _loggerMock = new Mock<ILogger<ProductService>>();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepoMock.Object);

        _sut = new ProductService(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductExists_ReturnsProductDto()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            ProductName = "Test Laptop",
            CreatedBy = "Admin",
            CreatedOn = DateTime.UtcNow,
            Items = new List<Item> { new() { Id = 10, ProductId = 1, Quantity = 5 } }
        };

        _productRepoMock.Setup(r => r.GetWithItemsByIdAsync(1, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.ProductName.Should().Be("Test Laptop");
        result.Items.Should().HaveCount(1);
        result.Items[0].Quantity.Should().Be(5);
    }

    [Fact]
    public async Task GetByIdAsync_WhenProductDoesNotExist_ThrowsNotFoundException()
    {
        // Arrange
        _productRepoMock.Setup(r => r.GetWithItemsByIdAsync(999, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act
        Func<Task> act = async () => await _sut.GetByIdAsync(999);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_ValidDto_CreatesAndReturnsDto()
    {
        // Arrange
        var createDto = new CreateProductDto
        {
            ProductName = "Mechanical Keyboard",
            CreatedBy = "Tester",
            Items = new List<CreateItemDto> { new() { Quantity = 2 } }
        };

        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _sut.CreateAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.ProductName.Should().Be("Mechanical Keyboard");
        result.CreatedBy.Should().Be("Tester");
        _productRepoMock.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenProductExists_UpdatesSuccessfully()
    {
        // Arrange
        var existingProduct = new Product
        {
            Id = 5,
            ProductName = "Old Name",
            CreatedBy = "Author",
            CreatedOn = DateTime.UtcNow
        };

        _productRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);
        _productRepoMock.Setup(r => r.GetWithItemsByIdAsync(5, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingProduct);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var updateDto = new UpdateProductDto
        {
            ProductName = "Updated Name",
            ModifiedBy = "Editor"
        };

        // Act
        var result = await _sut.UpdateAsync(5, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.ProductName.Should().Be("Updated Name");
        existingProduct.ModifiedBy.Should().Be("Editor");
        existingProduct.ModifiedOn.Should().NotBeNull();
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenProductExists_RemovesProduct()
    {
        // Arrange
        var product = new Product { Id = 3, ProductName = "To Delete" };
        _productRepoMock.Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        await _sut.DeleteAsync(3);

        // Assert
        _productRepoMock.Verify(r => r.Remove(product), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
