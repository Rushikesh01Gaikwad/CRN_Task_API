using Domain.Entities;
using FluentAssertions;
using Infrastructure.Data;
using Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Infrastructure.Tests;

public class ProductRepositoryTests
{
    private ApplicationDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task GetPagedAsync_ShouldReturnPagedResultsWithTotalCount()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        context.Products.AddRange(
            new Product { ProductName = "Monitor A", CreatedBy = "John", CreatedOn = DateTime.UtcNow.AddMinutes(-10) },
            new Product { ProductName = "Monitor B", CreatedBy = "John", CreatedOn = DateTime.UtcNow.AddMinutes(-5) },
            new Product { ProductName = "Desk Lamp", CreatedBy = "Sara", CreatedOn = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var repo = new ProductRepository(context);

        // Act
        var (items, totalCount) = await repo.GetPagedAsync(page: 1, pageSize: 2);

        // Assert
        totalCount.Should().Be(3);
        items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetWithItemsByIdAsync_ShouldIncludeRelatedItems()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var product = new Product
        {
            ProductName = "Smartphone",
            CreatedBy = "Tester",
            CreatedOn = DateTime.UtcNow,
            Items = new List<Item>
            {
                new() { Quantity = 3 },
                new() { Quantity = 7 }
            }
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var repo = new ProductRepository(context);

        // Act
        var result = await repo.GetWithItemsByIdAsync(product.Id, asNoTracking: true);

        // Assert
        result.Should().NotBeNull();
        result!.ProductName.Should().Be("Smartphone");
        result.Items.Should().HaveCount(2);
    }
}
