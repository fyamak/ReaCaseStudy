using System.Linq.Expressions;
using Infrastructure.Data.Postgres.EntityFramework;
using Infrastructure.Data.Postgres.Repositories.Base.Interface;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Postgres.Repositories.Base;

public abstract class Repository<TEntity, TId> : IRepository<TEntity, TId> where TEntity : class
{
    protected readonly PostgresContext PostgresContext;

    protected Repository(PostgresContext postgresContext)
    {
        PostgresContext = postgresContext;
    }

    public virtual async ValueTask<TEntity?> GetByIdAsync(TId id, bool tracked = false)
    {
        var entity = await PostgresContext.Set<TEntity>().FindAsync(id);

        if (!tracked && entity != null)
        {
            PostgresContext.Entry(entity).State = EntityState.Detached;
        }

        return entity;
    }

    public virtual async Task<IList<TEntity>> GetAllAsync(bool tracked = false)
    {
        if (!tracked)
        {
            return await PostgresContext.Set<TEntity>().AsNoTracking().ToListAsync();
        }

        return await PostgresContext.Set<TEntity>().ToListAsync();
    }

    public virtual async Task<IList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, bool tracked = false)
    {
        if (!tracked)
        {
            return await PostgresContext.Set<TEntity>().AsNoTracking().Where(predicate).ToListAsync();
        }

        return await PostgresContext.Set<TEntity>().Where(predicate).ToListAsync();
    }

    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await PostgresContext.Set<TEntity>().CountAsync(predicate);
    }

    public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate,
        bool                                                                                tracked = false)
    {
        if (!tracked)
        {
            return await PostgresContext.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(predicate);
        }

        return await PostgresContext.Set<TEntity>().FirstOrDefaultAsync(predicate);
    }

    public virtual async Task AddAsync(TEntity entity)
    {
        await PostgresContext.Set<TEntity>().AddAsync(entity);
    }

    public virtual async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        await PostgresContext.Set<TEntity>().AddRangeAsync(entities);
    }

    public virtual async Task RemoveById(TId id)
    {
        var entity = await GetByIdAsync(id);

        if (entity != null)
        {
            Remove(entity);
        }
    }

    public virtual void Remove(TEntity entity)
    {
        PostgresContext.Set<TEntity>().Remove(entity);
    }

    public virtual void RemoveRange(IEnumerable<TEntity> entities)
    {
        PostgresContext.Set<TEntity>().RemoveRange(entities);
    }
}
