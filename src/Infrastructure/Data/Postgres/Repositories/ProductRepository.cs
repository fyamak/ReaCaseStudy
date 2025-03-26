using Infrastructure.Data.Postgres.Entities;
using Infrastructure.Data.Postgres.EntityFramework;
using Infrastructure.Data.Postgres.Repositories.Base;
using Infrastructure.Data.Postgres.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Postgres.Repositories
{
    public class ProductRepository : TrackedEntityRepository<Product, int>, IProductRepository
    {
        public ProductRepository(PostgresContext postgresContext) : base(postgresContext)
        {
        }
        public async Task<int> Update(Product product)
        {
            var trackedEntity = PostgresContext.Products.Local.FirstOrDefault(p => p.Id == product.Id);
            if (trackedEntity != null)
            {
                PostgresContext.Entry(trackedEntity).State = EntityState.Detached;
            }

            PostgresContext.Products.Update(product);
            return await PostgresContext.SaveChangesAsync();
        }
    }
}
