using Infrastructure.Data.Postgres.Entities;
using Infrastructure.Data.Postgres.EntityFramework;
using Infrastructure.Data.Postgres.Repositories.Base;
using Infrastructure.Data.Postgres.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

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

        public async Task<int> Update(Order order)
        {
            PostgresContext.Orders.Update(order);
            return await PostgresContext.SaveChangesAsync();
        }

        public async Task<IList<Order>> GetLastProcessedOrdersAsync(int count)
        {
            return await PostgresContext.Orders
                .Include(o => o.Product)
                .Where(o => o.IsDeleted)
                .OrderByDescending(o => o.UpdatedAt)
                .Take(count)
                .AsNoTracking()
                .ToListAsync();
        }


        public async Task<(IList<Order> Items, int TotalCount)> GetPagedOrdersAsync(
            int pageNumber,
            int pageSize,
            bool isDeleted,
            string? search = null,
            string? type = null,
            bool tracked = false)
        {
            var query = PostgresContext.Orders
                .Include(p => p.Product)
                .Include(o => o.Organization)
                .AsQueryable();

            if (isDeleted)
            {
                query = query.Where(x => x.IsDeleted);
            } else {
                query = query.Where(x => !x.IsDeleted);

            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(order =>
                    order.Product.Name.ToLower().Contains(search) ||
                    order.Organization.Name.ToLower().Contains(search));
            }


            if (!string.IsNullOrWhiteSpace(type))
            {
                var lowerType = type.ToLower();
                query = query.Where(order => order.Type.ToLower() == lowerType);
            }
            else
            {
                query = query.Where(order =>
                    order.Type.ToLower() == "sale" || order.Type.ToLower() == "supply");
            }


            if (!tracked)
            {
                query = query.AsNoTracking();
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(o => o.Date)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

    }
}
