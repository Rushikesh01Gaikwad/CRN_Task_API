using Application.DTOs.Products;
using Application.Validators;
using FluentAssertions;
using Xunit;

namespace Application.Tests;

public class ProductValidatorTests
{
    private readonly CreateProductValidator _createValidator = new();
    private readonly UpdateProductValidator _updateValidator = new();

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("   ")]
    public void CreateValidator_WhenProductNameIsEmpty_ShouldHaveValidationError(string? invalidName)
    {
        var model = new CreateProductDto { ProductName = invalidName!, CreatedBy = "Author" };
        var result = _createValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductDto.ProductName));
    }

    [Fact]
    public void CreateValidator_WhenProductNameExceeds255Chars_ShouldHaveValidationError()
    {
        var model = new CreateProductDto
        {
            ProductName = new string('A', 256),
            CreatedBy = "Author"
        };
        var result = _createValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateProductDto.ProductName));
    }

    [Fact]
    public void CreateValidator_WhenValid_ShouldBeValid()
    {
        var model = new CreateProductDto
        {
            ProductName = "Valid Product Name",
            CreatedBy = "Admin",
            Items = new List<CreateItemDto> { new() { Quantity = 10 } }
        };
        var result = _createValidator.Validate(model);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void UpdateValidator_WhenModifiedByIsEmpty_ShouldHaveValidationError(string? invalidModifiedBy)
    {
        var model = new UpdateProductDto { ProductName = "Valid", ModifiedBy = invalidModifiedBy! };
        var result = _updateValidator.Validate(model);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateProductDto.ModifiedBy));
    }
}
