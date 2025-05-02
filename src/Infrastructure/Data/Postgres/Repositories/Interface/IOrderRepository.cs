using Infrastructure.Data.Postgres.Entities;
using Infrastructure.Data.Postgres.Repositories.Base.Interface;

namespace Infrastructure.Data.Postgres.Repositories.Interface
{
    public interface IOrderRepository : ITrackedEntityRepository<Order, int>
    {
        public Task<int> SoftDelete(Order order);
        public Task<int> Update(Order order);
    }
}
