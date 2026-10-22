using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedOnAdd();

        builder.Property(p => p.ProductName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.CreatedOn)
            .IsRequired()
            .HasColumnType("datetime");

        builder.Property(p => p.ModifiedBy)
            .HasMaxLength(100);

        builder.Property(p => p.ModifiedOn)
            .HasColumnType("datetime");

        // Index on ProductName for fast lookups and search
        builder.HasIndex(p => p.ProductName);

        builder.HasMany(p => p.Items)
            .WithOne(i => i.Product)
            .HasForeignKey(i => i.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> builder)
    {
        builder.ToTable("Items");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id).ValueGeneratedOnAdd();

        builder.Property(i => i.Quantity)
            .IsRequired();

        builder.HasIndex(i => i.ProductId);
    }
}
