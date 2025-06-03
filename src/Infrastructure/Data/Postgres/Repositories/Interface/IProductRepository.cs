using Infrastructure.Data.Postgres.Entities;
using Infrastructure.Data.Postgres.Repositories.Base.Interface;

namespace Infrastructure.Data.Postgres.Repositories.Interface
{
    public interface IProductRepository : ITrackedEntityRepository<Product, int>
    {
        public Task<int> Update(Product product);
        public Task<(IList<Product> Items, int TotalCount)> GetPagedProductsAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        int? categoryId = null,
        bool includeDeleted = false,
        bool tracked = false);

        public Task<(IList<Order> Items, int TotalCount)> GetPagedTransactionAsync(
             int pageNumber,
             int pageSize,
             DateTime startDate,
             DateTime endDate,
             bool includeFailures,
             int? productId = null,
             bool tracked = false);
    }
}
