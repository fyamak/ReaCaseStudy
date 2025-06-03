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
            PostgresContext.Products.Update(product);
            return await PostgresContext.SaveChangesAsync();
        }

        public async Task<(IList<Product> Items, int TotalCount)> GetPagedProductsAsync(
            int pageNumber,
            int pageSize,
            string? search = null,
            int? categoryId = null,
            bool includeDeleted = false,
            bool tracked = false)
        {
            var query = PostgresContext.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (!includeDeleted)
            {
                query = query.Where(x => !x.IsDeleted);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(prdocut =>
                    prdocut.Name.ToLower().Contains(search) ||
                    prdocut.SKU.ToLower().Contains(search));
            }


            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            if (!tracked)
            {
                query = query.AsNoTracking();
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.TotalQuantity)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }


        public async Task<(IList<Order> Items, int TotalCount)> GetPagedTransactionAsync(
            int pageNumber,
            int pageSize,
            DateTime startDate,
            DateTime endDate,
            bool includeFailures,
            int? productId = null,
            bool tracked = false)
        {
            var query = PostgresContext.Orders
                .Include(p => p.Product)
                .Include(o => o.Organization)
                .Where(x => x.IsDeleted &&
                            x.Date >= startDate &&
                            x.Date <= endDate);

            if (!includeFailures)
            {
                query = query.Where(x => x.IsSuccessfull == true);
            }

            if (productId.HasValue)
            {
                query = query.Where(x => x.ProductId == productId.Value);
            }

            if (!tracked)
            {
                query = query.AsNoTracking();
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.Date)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

    }
}
