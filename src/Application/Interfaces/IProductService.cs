using Application.DTOs.Common;
using Application.DTOs.Products;

namespace Application.Interfaces;

public interface IProductService
{
    Task<PagedResultDto<ProductDto>> GetPagedAsync(int page, int pageSize, string? searchTerm = null, CancellationToken cancellationToken = default);
    Task<ProductDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateAsync(CreateProductDto dto, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateAsync(int id, UpdateProductDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
