using Business.RequestHandlers.Product;
using Infrastructure.Data.Postgres.Entities;
using Infrastructure.Data.Postgres.EntityFramework;
using Shared.Models.Results;

namespace Business.Test.RequestHandlers.Product;

public class GetDashboardInformationTests : BaseHandlerTest
{
    public GetDashboardInformationTests()
    {
        BuildContainer();
    }

    [Fact]
    public async Task GetDashboardInformation_Success_Test()
    {
        const double SUPPLY_PRICE = 5;
        const int SUPPLY_QUANTITY = 100;
        const double SALE_PRICE = 10;
        const int SALE_QUANTITY = 5;

        var category = new Infrastructure.Data.Postgres.Entities.Category { Id = 1, Name = "Electronics" };
        var organization = new Infrastructure.Data.Postgres.Entities.Organization { Id = 1, Name = "Test Organization", Email = "test@example.com", Phone = "(555) 555-5555", Address = "Test Address" };
        var product = new Infrastructure.Data.Postgres.Entities.Product
        {
            Id = 1,
            Name = "Test Name",
            SKU = "Test SKU",
            CategoryId = category.Id,
            Category = category,
            TotalQuantity = 0
        };

        var productSupply = new ProductSupply
        {
            Id = 1,
            OrganizationId = organization.Id,
            Organization = organization,
            ProductId = product.Id,
            Product = product,
            Price = SUPPLY_PRICE,
            Quantity = SUPPLY_QUANTITY,
            RemainingQuantity = SUPPLY_QUANTITY,
            Date = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        var productSale = new ProductSale
        {
            Id = 1,
            OrganizationId = organization.Id,
            Organization = organization,
            ProductId = product.Id,
            Product = product,
            Price = SALE_PRICE,
            Quantity = SALE_QUANTITY,
            Date = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await PostgresContext.Products.AddAsync(product);
        await PostgresContext.ProductSupplies.AddAsync(productSupply);
        await PostgresContext.ProductSales.AddAsync(productSale);
        await PostgresContext.SaveChangesAsync();

        var request = new GetDashboardInformation.GetDashboardInformationRequest();
        var response = await Mediator.Send(request);

        Assert.NotNull(response);
        Assert.Equal(ResultStatus.Success, response.Status);

        Assert.Equal(SUPPLY_PRICE * SUPPLY_QUANTITY, response.Data.MonthlySupplyExpense);
        Assert.Equal(SALE_PRICE * SALE_QUANTITY, response.Data.MonthlySalesRevenue);
        Assert.Equal(1, response.Data.ProductCount);
        Assert.Equal(1, response.Data.LowStockItems);

    }

    [Fact]
    public async Task GetDashboardInformation_EmptyDatabase_Test()
    {
        var request = new GetDashboardInformation.GetDashboardInformationRequest();
        var response = await Mediator.Send(request);

        Assert.NotNull(response);
        Assert.Equal(ResultStatus.Success, response.Status);

        Assert.Equal(0, response.Data.ProductCount);
        Assert.Equal(0, response.Data.LowStockItems);
        Assert.Equal(0, response.Data.ActiveSupplies);
        Assert.Equal(0, response.Data.OrganizationCount);
        Assert.Equal(0, response.Data.PendingOrders);
        Assert.Equal(0, response.Data.MonthlySupplyExpense);
        Assert.Equal(0, response.Data.MonthlySalesRevenue);
        Assert.Equal(0, response.Data.MonthlyProfit);
        Assert.Empty(response.Data.LastProcessedOrders);
    }

}
