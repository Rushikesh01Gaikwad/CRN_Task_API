using Application.DTOs.Products;
using Domain.Entities;

namespace Application.Mapping;

public static class ProductMappingExtensions
{
    public static ProductDto ToDto(this Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            ProductName = product.ProductName,
            CreatedBy = product.CreatedBy,
            CreatedOn = product.CreatedOn,
            ModifiedBy = product.ModifiedBy,
            ModifiedOn = product.ModifiedOn,
            Items = product.Items.Select(i => i.ToDto()).ToList()
        };
    }

    public static ItemDto ToDto(this Item item)
    {
        return new ItemDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            Quantity = item.Quantity
        };
    }

    public static Product ToEntity(this CreateProductDto dto)
    {
        var product = new Product
        {
            ProductName = dto.ProductName.Trim(),
            CreatedBy = dto.CreatedBy.Trim(),
            CreatedOn = DateTime.UtcNow,
            ModifiedBy = null,
            ModifiedOn = null
        };

        if (dto.Items != null && dto.Items.Count > 0)
        {
            foreach (var item in dto.Items)
            {
                product.Items.Add(new Item
                {
                    Quantity = item.Quantity
                });
            }
        }

        return product;
    }

    public static Item ToEntity(this CreateItemDto dto, int productId)
    {
        return new Item
        {
            ProductId = productId,
            Quantity = dto.Quantity
        };
    }
}
