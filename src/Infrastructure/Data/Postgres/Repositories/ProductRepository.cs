using Infrastructure.Data.Postgres.Entities;
using Infrastructure.Data.Postgres.EntityFramework;
using Infrastructure.Data.Postgres.Repositories.Base;
using Infrastructure.Data.Postgres.Repositories.Interface;

namespace Infrastructure.Data.Postgres.Repositories
{
    public class ProductRepository : TrackedEntityRepository<Product, int>, IProductRepository
    {
        public ProductRepository(PostgresContext postgresContext) : base(postgresContext)
        {
        }
        public async Task<int> Update(Product product)
        {

            PostgresContext.Products.Update(product);
            return await PostgresContext.SaveChangesAsync();
        }
    }
}
