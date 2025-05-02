using Infrastructure.Data.Postgres.Entities.Base;

namespace Infrastructure.Data.Postgres.Entities;

public class Category : TrackedBaseEntity<int>
{
    public string Name { get; set; } = default!;
}
