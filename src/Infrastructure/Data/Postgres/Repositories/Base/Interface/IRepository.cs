using System.Linq.Expressions;

namespace Infrastructure.Data.Postgres.Repositories.Base.Interface;

public interface IRepository<TEntity, in TId> where TEntity : class
{
    ValueTask<TEntity?>  GetByIdAsync(TId                                    id, bool tracked = false);
    Task<IList<TEntity>> GetAllAsync(bool                                    tracked                 = false);
    Task<IList<TEntity>> FindAsync(Expression<Func<TEntity, bool>>           predicate, bool tracked = false);
    Task<TEntity?>       FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, bool tracked = false);
    Task                 AddAsync(TEntity                                    entity);
    Task                 AddRangeAsync(IEnumerable<TEntity>                  entities);
    Task                 RemoveById(TId                                      id);
    void                 Remove(TEntity                                      entity);
    void                 RemoveRange(IEnumerable<TEntity>                    entities);
    Task<int>            CountAsync(Expression<Func<TEntity, bool>>          predicate);
}
