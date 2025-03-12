using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.RequestHandlers.Product;
using Shared.Models.Results;

namespace Business.Test.RequestHandlers.Product;

public class TransactionTests : BaseHandlerTest
{
    public TransactionTests()
    {
        BuildContainer();
    }

    [Fact]
    public async Task Transaction_Success_When_Valid_Date_Range_Without_ProductId_Test()
    {
        var productToAdd = new Infrastructure.Data.Postgres.Entities.Product { Name = "Test Product" };
        await PostgresContext.Products.AddAsync(productToAdd);
        var supplyToAdd = new Infrastructure.Data.Postgres.Entities.ProductSupply { ProductId = productToAdd.Id, Date = DateTime.UtcNow, Quantity = 100, RemainingQuantity = 100 };
        await PostgresContext.ProductSupplies.AddAsync(supplyToAdd);
        var saleToAdd = new Infrastructure.Data.Postgres.Entities.ProductSale { ProductId = productToAdd.Id, Date = DateTime.UtcNow, Quantity = 100 };
        await PostgresContext.ProductSales.AddAsync(saleToAdd);
        await PostgresContext.SaveChangesAsync();

        var request = new Transaction.TransactionRequest { StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1) };
        var response = await Mediator.Send(request);
        
        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal(response.Data.Count, 2);
    }

    [Fact]
    public async Task Transaction_Success_When_Valid_Date_Range_And_ProductId_Are_Provided_Test()
    {
        var productToAdd = new Infrastructure.Data.Postgres.Entities.Product { Name = "Test Product" };
        await PostgresContext.Products.AddAsync(productToAdd);
        var supplyToAdd = new Infrastructure.Data.Postgres.Entities.ProductSupply { ProductId = productToAdd.Id, Date = DateTime.UtcNow, Quantity = 100, RemainingQuantity = 100 };
        await PostgresContext.ProductSupplies.AddAsync(supplyToAdd);
        var saleToAdd = new Infrastructure.Data.Postgres.Entities.ProductSale { ProductId = productToAdd.Id, Date = DateTime.UtcNow, Quantity = 100 };
        await PostgresContext.ProductSales.AddAsync(saleToAdd);
        await PostgresContext.SaveChangesAsync();

        var request = new Transaction.TransactionRequest { ProductId = 2, StartDate = DateTime.UtcNow.AddDays(-1), EndDate = DateTime.UtcNow.AddDays(1) };
        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.NotNull(response);
        Assert.Empty(response.Data);
    }


    [Fact]
    public async Task Transaction_Fail_When_Invalid_Date_Range_Provided_Test()
    {
        var request = new Transaction.TransactionRequest { StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(-1) };
        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal(response.Message, "End date cannot be earlier than start date.");
        Assert.Null(response.Data);
    }

    [Fact]
    public async Task Transaction_Returns_Transactions_Sorted_By_Date_Test()
    {
        var productToAdd = new Infrastructure.Data.Postgres.Entities.Product { Name = "Test Product" };
        await PostgresContext.Products.AddAsync(productToAdd);
        var supplyToAdd1 = new Infrastructure.Data.Postgres.Entities.ProductSupply { ProductId = productToAdd.Id, Date = DateTime.UtcNow.AddDays(1), Quantity = 100, RemainingQuantity = 100 };
        await PostgresContext.ProductSupplies.AddAsync(supplyToAdd1);
        var supplyToAdd2 = new Infrastructure.Data.Postgres.Entities.ProductSupply { ProductId = productToAdd.Id, Date = DateTime.UtcNow, Quantity = 150, RemainingQuantity = 150 };
        await PostgresContext.ProductSupplies.AddAsync(supplyToAdd2);
        await PostgresContext.SaveChangesAsync();

        var request = new Transaction.TransactionRequest { StartDate = DateTime.UtcNow.AddDays(-2), EndDate = DateTime.UtcNow.AddDays(2) };
        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.True(response.Data[0].Date < response.Data[1].Date);
    }
}
