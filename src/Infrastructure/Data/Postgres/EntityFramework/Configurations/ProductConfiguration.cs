using Infrastructure.Data.Postgres.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Data.Postgres.EntityFramework.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => x.SKU).IsUnique();

            builder.Property(x => x.Name).IsRequired();
            builder.Property(x => x.SKU).IsRequired();

            builder.Property(x => x.IsDeleted).HasDefaultValue(false).IsRequired();
        }
    }
}
