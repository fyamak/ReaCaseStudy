using Infrastructure.Data.Postgres.Entities;
using Infrastructure.Data.Postgres.EntityFramework;
using Infrastructure.Data.Postgres.Repositories.Base;
using Infrastructure.Data.Postgres.Repositories.Interface;

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
}
