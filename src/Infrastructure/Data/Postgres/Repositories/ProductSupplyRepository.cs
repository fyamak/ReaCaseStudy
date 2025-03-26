using Infrastructure.Data.Postgres.Entities;
using Infrastructure.Data.Postgres.EntityFramework;
using Infrastructure.Data.Postgres.Repositories.Base;
using Infrastructure.Data.Postgres.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Postgres.Repositories
{
    public class ProductSupplyRepository : TrackedEntityRepository<ProductSupply, int>, IProductSupplyRepository
    {
        public ProductSupplyRepository(PostgresContext postgresContext) : base(postgresContext)
        {
        }
        public async Task<int> Update(ProductSupply productSupply)
        {
            var trackedEntity = PostgresContext.ProductSupplies.Local.FirstOrDefault(ps => ps.Id == productSupply.Id);
            if (trackedEntity != null)
            {
                PostgresContext.Entry(trackedEntity).State = EntityState.Detached;
            }

            PostgresContext.ProductSupplies.Update(productSupply);
            return await PostgresContext.SaveChangesAsync();
        }
    }
}
