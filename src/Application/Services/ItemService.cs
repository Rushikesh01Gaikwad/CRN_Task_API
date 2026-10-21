using Application.DTOs.Products;
using Application.Interfaces;
using Application.Mapping;
using Domain.Entities;
using Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class ItemService : IItemService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ItemService> _logger;

    public ItemService(IUnitOfWork unitOfWork, ILogger<ItemService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ItemDto>> GetByProductIdAsync(int productId, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);
        if (product == null)
        {
            throw new NotFoundException(nameof(Product), productId);
        }

        var items = await _unitOfWork.Items.GetByProductIdAsync(productId, asNoTracking: true, cancellationToken);
        return items.Select(i => i.ToDto()).ToList();
    }

    public async Task<ItemDto> GetByIdAsync(int productId, int itemId, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.Items.GetByIdAndProductIdAsync(itemId, productId, asNoTracking: true, cancellationToken);
        if (item == null)
        {
            throw new NotFoundException(nameof(Item), itemId);
        }

        return item.ToDto();
    }

    public async Task<ItemDto> CreateAsync(int productId, CreateItemDto dto, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(productId, cancellationToken);
        if (product == null)
        {
            throw new NotFoundException(nameof(Product), productId);
        }

        var item = dto.ToEntity(productId);
        await _unitOfWork.Items.AddAsync(item, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Added Item {ItemId} with Quantity {Quantity} to Product {ProductId}", item.Id, item.Quantity, productId);
        return item.ToDto();
    }

    public async Task<ItemDto> UpdateAsync(int productId, int itemId, UpdateItemDto dto, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.Items.GetByIdAndProductIdAsync(itemId, productId, asNoTracking: false, cancellationToken);
        if (item == null)
        {
            throw new NotFoundException(nameof(Item), itemId);
        }

        item.Quantity = dto.Quantity;
        _unitOfWork.Items.Update(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated Item {ItemId} on Product {ProductId}", itemId, productId);
        return item.ToDto();
    }

    public async Task DeleteAsync(int productId, int itemId, CancellationToken cancellationToken = default)
    {
        var item = await _unitOfWork.Items.GetByIdAndProductIdAsync(itemId, productId, asNoTracking: false, cancellationToken);
        if (item == null)
        {
            throw new NotFoundException(nameof(Item), itemId);
        }

        _unitOfWork.Items.Remove(item);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted Item {ItemId} from Product {ProductId}", itemId, productId);
    }
}
