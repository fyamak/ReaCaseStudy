using System.Linq.Expressions;
using Infrastructure.Data.Postgres.Entities.Base;
using Infrastructure.Data.Postgres.EntityFramework;
using Infrastructure.Data.Postgres.Repositories.Base.Interface;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Postgres.Repositories.Base;

public abstract class TrackedEntityRepository<TEntity, TId> : ITrackedEntityRepository<TEntity, TId>
    where TEntity : TrackedBaseEntity<TId>
{
    protected readonly PostgresContext PostgresContext;

    protected TrackedEntityRepository(PostgresContext postgresContext)
    {
        PostgresContext = postgresContext;
    }

    public async Task<TEntity?> GetByIdAsync(TId id, bool includeDeleted = false, bool tracked = false)
    {
        var entity = await PostgresContext.Set<TEntity>().FindAsync(id);

        if (entity != null)
        {
            if (!includeDeleted && entity.IsDeleted)
            {
                entity = null;
            }
            else
            {
                if (!tracked)
                {
                    PostgresContext.Entry(entity).State = EntityState.Detached;
                }
            }
        }

        return entity;
    }

    public async Task<IList<TEntity>> GetAllAsync(bool includeDeleted = false, bool tracked = false)
    {
        var query = includeDeleted
            ? PostgresContext.Set<TEntity>()
            : PostgresContext.Set<TEntity>().Where(x => !x.IsDeleted);

        if (!tracked)
        {
            return await query.AsNoTracking().ToListAsync();
        }

        return await query.ToListAsync();
    }

    public async Task<IList<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, bool includeDeleted = false,
        bool                                                                    tracked = false)
    {
        var query = PostgresContext.Set<TEntity>().Where(predicate);

        if (!includeDeleted)
        {
            query = query.Where(x => !x.IsDeleted);
        }

        if (!tracked)
        {
            return await query.AsNoTracking().ToListAsync();
        }

        return await query.ToListAsync();
    }

    public async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate,
        bool                                                                        includeDeleted = false,
        bool                                                                        tracked        = false)
    {
        var query = PostgresContext.Set<TEntity>().Where(predicate);

        if (!includeDeleted)
        {
            query = query.Where(x => !x.IsDeleted);
        }

        if (!tracked)
        {
            return await query.AsNoTracking().FirstOrDefaultAsync();
        }

        return await query.FirstOrDefaultAsync();
    }


    public Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, bool includeDeleted = false)
    {
        var query = PostgresContext.Set<TEntity>().Where(predicate);

        if (!includeDeleted)
        {
            query = query.Where(x => !x.IsDeleted);
        }

        return query.CountAsync();
    }

    public async Task AddAsync(TEntity entity)
    {
        await PostgresContext.Set<TEntity>().AddAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<TEntity> entities)
    {
        await PostgresContext.Set<TEntity>().AddRangeAsync(entities);
    }

    public async Task RemoveByIdAsync(TId id)
    {
        var entity = await GetByIdAsync(id, tracked: true);

        if (entity != null)
        {
            entity.IsDeleted = true;
        }
    }

    public async Task RemoveRangeAsync(IEnumerable<TEntity> entities)
    {
        var entityIdsToRemove = entities.Select(x => x.Id);

        var entitiesToRemove = await FindAsync(x => entityIdsToRemove.Contains(x.Id), tracked: true);

        foreach (var entityToRemove in entitiesToRemove)
        {
            entityToRemove.IsDeleted = true;
        }
    }
}
