using Infrastructure.Data.Postgres.Repositories.Interface;

namespace Infrastructure.Data.Postgres;

public interface IUnitOfWork : IDisposable
{
    IUserRepository      Users      { get; }
    IUserTokenRepository UserTokens { get; }
    IProductRepository Products { get; }
    IProductSaleRepository ProductSales { get; }
    IProductSupplyRepository ProductSupplies { get; }
    Task<int>            CommitAsync();
}
