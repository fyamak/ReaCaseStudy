﻿using Infrastructure.Data.Postgres.Entities;
using Infrastructure.Data.Postgres.EntityFramework;
using Infrastructure.Data.Postgres.Repositories.Base;
using Infrastructure.Data.Postgres.Repositories.Interface;

namespace Infrastructure.Data.Postgres.Repositories
{
    public class ProductSaleRepository : TrackedEntityRepository<ProductSale, int>, IProductSaleRepository
    {
        public ProductSaleRepository(PostgresContext postgresContext) : base(postgresContext)
        {
        }
    }
}
