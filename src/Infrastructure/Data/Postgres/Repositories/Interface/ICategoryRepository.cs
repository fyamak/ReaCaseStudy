using Infrastructure.Data.Postgres.Entities;
using Infrastructure.Data.Postgres.Repositories.Base.Interface;

namespace Infrastructure.Data.Postgres.Repositories.Interface;

public interface ICategoryRepository : ITrackedEntityRepository<Category, int>
{
    public Task<int> SoftDelete(Category category);
}
