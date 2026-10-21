using Domain.Entities;

namespace Application.Interfaces;

public interface IItemRepository : IRepository<Item>
{
    Task<IReadOnlyList<Item>> GetByProductIdAsync(int productId, bool asNoTracking = true, CancellationToken cancellationToken = default);
    Task<Item?> GetByIdAndProductIdAsync(int id, int productId, bool asNoTracking = false, CancellationToken cancellationToken = default);
}
