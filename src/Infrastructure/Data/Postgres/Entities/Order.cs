using Infrastructure.Data.Postgres.Entities.Base;

namespace Infrastructure.Data.Postgres.Entities
{
    public class Order : TrackedBaseEntity<int>
    {
        public int ProductId { get; set; }
        public int OrganizationId { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
    }
}
