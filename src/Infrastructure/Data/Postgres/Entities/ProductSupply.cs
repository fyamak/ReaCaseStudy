using Infrastructure.Data.Postgres.Entities.Base;

namespace Infrastructure.Data.Postgres.Entities
{
    public class ProductSupply : TrackedBaseEntity<int>
    {
        public int ProductId { get; set; }
        public int OrganizationId { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public DateTime Date { get; set; }
        public int RemainingQuantity { get; set; }
        public Product Product { get; set; } = default!;
        public Organization Organization { get; set; } = default!;

    }
}
