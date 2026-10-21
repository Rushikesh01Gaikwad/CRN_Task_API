using Application.DTOs.Products;

namespace Application.Interfaces;

public interface IItemService
{
    Task<IReadOnlyList<ItemDto>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default);
    Task<ItemDto> GetByIdAsync(int productId, int itemId, CancellationToken cancellationToken = default);
    Task<ItemDto> CreateAsync(int productId, CreateItemDto dto, CancellationToken cancellationToken = default);
    Task<ItemDto> UpdateAsync(int productId, int itemId, UpdateItemDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int productId, int itemId, CancellationToken cancellationToken = default);
}
