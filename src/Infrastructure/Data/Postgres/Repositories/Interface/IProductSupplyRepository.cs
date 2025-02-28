using Infrastructure.Data.Postgres.Entities;
using Infrastructure.Data.Postgres.Repositories.Base.Interface;

namespace Infrastructure.Data.Postgres.Repositories.Interface
{
    public interface IProductSupplyRepository : ITrackedEntityRepository<ProductSupply, int>
    {
        public Task<int> Update(ProductSupply productSupply);
    }
}
