using Application.DTOs.Common;
using Application.DTOs.Products;
using Application.Interfaces;
using Application.Mapping;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IUnitOfWork unitOfWork, ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PagedResultDto<ProductDto>> GetPagedAsync(
        int page,
        int pageSize,
        string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        _logger.LogInformation("Fetching paged products: Page {Page}, PageSize {PageSize}", page, pageSize);

        var (items, totalCount) = await _unitOfWork.Products.GetPagedAsync(page, pageSize, searchTerm, cancellationToken);

        return new PagedResultDto<ProductDto>
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            Items = items.Select(p => p.ToDto()).ToList()
        };
    }

    public async Task<ProductDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetWithItemsByIdAsync(id, asNoTracking: true, cancellationToken);
        if (product == null)
        {
            throw new NotFoundException(nameof(Product), id);
        }

        return product.ToDto();
    }

    public async Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating new product: {ProductName}", dto.ProductName);

        var product = dto.ToEntity();
        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created product with ID {ProductId}", product.Id);
        return product.ToDto();
    }

    public async Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating product {ProductId}", id);

        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            throw new NotFoundException(nameof(Product), id);
        }

        product.ProductName = dto.ProductName.Trim();
        product.ModifiedBy = dto.ModifiedBy.Trim();
        product.ModifiedOn = DateTime.UtcNow;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Fetch refreshed product with items
        var updatedProduct = await _unitOfWork.Products.GetWithItemsByIdAsync(id, asNoTracking: true, cancellationToken);
        return (updatedProduct ?? product).ToDto();
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting product {ProductId}", id);

        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            throw new NotFoundException(nameof(Product), id);
        }

        _unitOfWork.Products.Remove(product);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted product with ID {ProductId}", id);
    }
}
