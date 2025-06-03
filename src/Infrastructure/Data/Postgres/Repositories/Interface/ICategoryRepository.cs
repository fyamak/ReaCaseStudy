using Infrastructure.Data.Postgres.Entities;
using Infrastructure.Data.Postgres.Repositories.Base.Interface;

namespace Infrastructure.Data.Postgres.Repositories.Interface;

public interface ICategoryRepository : ITrackedEntityRepository<Category, int>
{
    public Task<int> SoftDelete(Category category);
    public Task<(IList<Category> Items, int TotalCount)> GetPagedCategoriesAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        bool includeDeleted = false,
        bool tracked = false);
}
