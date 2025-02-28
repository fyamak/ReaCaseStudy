using System.Linq.Expressions;
using Infrastructure.Data.Postgres.Entities.Base;

namespace Infrastructure.Data.Postgres.Repositories.Base.Interface;

public interface ITrackedEntityRepository<TEntity, in TId> where TEntity : TrackedBaseEntity<TId>
{
    Task<TEntity?>       GetByIdAsync(TId id,                     bool includeDeleted = false, bool tracked = false);
    Task<IList<TEntity>> GetAllAsync(bool includeDeleted = false, bool tracked        = false);

    Task<IList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, bool includeDeleted = false,
        bool                                                       tracked = false);

    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, bool includeDeleted = false,
        bool                                                           tracked = false);

    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, bool includeDeleted = false);
    Task      AddAsync(TEntity                           entity);
    Task      AddRangeAsync(IEnumerable<TEntity>         entities);
    Task      RemoveByIdAsync(TId                        id);
    Task      RemoveRangeAsync(IEnumerable<TEntity>      entities);
}
