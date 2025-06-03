using Infrastructure.Data.Postgres.Entities;
using Infrastructure.Data.Postgres.EntityFramework;
using Infrastructure.Data.Postgres.Repositories.Base;
using Infrastructure.Data.Postgres.Repositories.Interface;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Postgres.Repositories;

public class OrganizationRepository : TrackedEntityRepository<Organization, int>, IOrganizationRepository
{
    public OrganizationRepository(PostgresContext postgresContext) : base(postgresContext)
    {
    }

    public async Task<int> SoftDelete(Organization organization)
    {
        organization.IsDeleted = true;
        PostgresContext.Organizations.Update(organization);
        return await PostgresContext.SaveChangesAsync();
    }

    public async Task<(IList<Organization> Items, int TotalCount)> GetPagedOrganizationsAsync(
        int pageNumber,
        int pageSize,
        string? search = null,
        bool includeDeleted = false,
        bool tracked = false)
    {
        var query = PostgresContext.Organizations.AsQueryable();

        if (!includeDeleted)
        {
            query = query.Where(x => !x.IsDeleted);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(org =>
                org.Name.ToLower().Contains(search) ||
                org.Email.ToLower().Contains(search) ||
                org.Address.ToLower().Contains(search));
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
