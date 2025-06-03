using Infrastructure.Data.Postgres.Entities.Base;

namespace Infrastructure.Data.Postgres.Entities;

public class Category : TrackedBaseEntity<int>
{
    public string Name { get; set; } = default!;
    public ICollection<Product> Products { get; set; } = new List<Product>();

}
