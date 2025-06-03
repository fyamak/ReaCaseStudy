using Infrastructure.Data.Postgres.Entities.Base;

namespace Infrastructure.Data.Postgres.Entities
{
    public class Product : TrackedBaseEntity<int>
    {
        public string Name { get; set; } = default!;
        public string SKU { get; set; } = default!;
        public int TotalQuantity { get; set; } = default!;
        public int CategoryId { get; set; } = default!;
        public Category Category { get; set; } = default!;
        public ICollection<ProductSupply> Supplies { get; set; } = new List<ProductSupply>();
        public ICollection<ProductSale> Sales { get; set; } = new List<ProductSale>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
