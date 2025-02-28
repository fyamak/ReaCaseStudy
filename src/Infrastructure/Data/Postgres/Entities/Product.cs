using Infrastructure.Data.Postgres.Entities.Base;

namespace Infrastructure.Data.Postgres.Entities
{
    public class Product : TrackedBaseEntity<int>
    {
        public string Name { get; set; } = default!;
        public ICollection<ProductSupply> Supplies { get; set; } = new List<ProductSupply>();
        public ICollection<ProductSale> Sales { get; set; } = new List<ProductSale>();
    }
}
