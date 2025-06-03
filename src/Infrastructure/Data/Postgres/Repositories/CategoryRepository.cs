using Infrastructure.Data.Postgres.Entities;
using Infrastructure.Data.Postgres.EntityFramework;
using Infrastructure.Data.Postgres.Repositories.Base;
using Infrastructure.Data.Postgres.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Postgres.Repositories;

public class CategoryRepository : TrackedEntityRepository<Category, int>, ICategoryRepository
{
    public CategoryRepository(PostgresContext postgresContext) : base(postgresContext)
    {
    }

    public async Task<int> SoftDelete(Category category)
    {
        category.IsDeleted = true;
        PostgresContext.Categories.Update(category);
        return await PostgresContext.SaveChangesAsync();
    }

    public async Task<(IList<Category> Items, int TotalCount)> GetPagedCategoriesAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        bool includeDeleted = false,
        bool tracked = false)
    {
        var query = PostgresContext.Categories.AsQueryable();

        if (!includeDeleted)
        {
            query = query.Where(x => !x.IsDeleted);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(category => category.Name.ToLower().Contains(search));
        }

        if (!tracked)
        {
            query = query.AsNoTracking();
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

}
