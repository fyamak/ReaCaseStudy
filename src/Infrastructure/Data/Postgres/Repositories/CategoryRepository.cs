using Infrastructure.Data.Postgres.Entities;
using Infrastructure.Data.Postgres.EntityFramework;
using Infrastructure.Data.Postgres.Repositories.Base;
using Infrastructure.Data.Postgres.Repositories.Interface;

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

}
