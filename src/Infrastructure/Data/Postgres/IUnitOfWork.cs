using Infrastructure.Data.Postgres.Repositories.Interface;

namespace Infrastructure.Data.Postgres;

public interface IUnitOfWork : IDisposable
{
    IUserRepository      Users      { get; }
    IUserTokenRepository UserTokens { get; }
    IProductRepository Products { get; }
    IProductSaleRepository ProductSales { get; }
    IProductSupplyRepository ProductSupplies { get; }
    IOrganizationRepository Organizations { get; }
    IOrderRepository Orders { get; }
    ICategoryRepository Categories { get; }
    Task<int>            CommitAsync();
}
