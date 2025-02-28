using Infrastructure.Data.Postgres.Entities;
using Infrastructure.Data.Postgres.Repositories.Base.Interface;

namespace Infrastructure.Data.Postgres.Repositories.Interface
{
    public interface IProductRepository : ITrackedEntityRepository<Product, int>
    {
        public Task<int> Update(Product product);
    }
}
