using Infrastructure.Data.Postgres.Entities;
using Infrastructure.Data.Postgres.EntityFramework;
using Infrastructure.Data.Postgres.Repositories.Base;
using Infrastructure.Data.Postgres.Repositories.Interface;

namespace Infrastructure.Data.Postgres.Repositories
{
    public class OrderRepository : TrackedEntityRepository<Order, int>, IOrderRepository
    {
        public OrderRepository(PostgresContext postgresContext) : base(postgresContext)
        {
        }

        public async Task<int> SoftDelete(Order order)
        {
            order.IsDeleted = true;
            PostgresContext.Orders.Update(order);
            return await PostgresContext.SaveChangesAsync();
        }

    }
}
