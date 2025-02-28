using Infrastructure.Data.Postgres.Entities;
using Infrastructure.Data.Postgres.EntityFramework.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Postgres.EntityFramework;

public class PostgresContext : DbContext
{
    public PostgresContext(DbContextOptions<PostgresContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new UserTokenConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new ProductSalesConfiguration());
        modelBuilder.ApplyConfiguration(new ProductSupplyConfiguration());

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserToken> UserTokens => Set<UserToken>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductSale> ProductSales => Set<ProductSale>();
    public DbSet<ProductSupply> ProductSupplies => Set<ProductSupply>();
}
