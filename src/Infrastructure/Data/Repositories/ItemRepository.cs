using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public class ItemRepository : Repository<Item>, IItemRepository
{
    public ItemRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Item>> GetByProductIdAsync(int productId, bool asNoTracking = true, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(i => i.ProductId == productId);
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<Item?> GetByIdAndProductIdAsync(int id, int productId, bool asNoTracking = false, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(i => i.Id == id && i.ProductId == productId);
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }
}
