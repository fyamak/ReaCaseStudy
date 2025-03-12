using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.RequestHandlers.Product;
using Infrastructure.Data.Postgres.Entities;
using Shared.Models.Results;

namespace Business.Test.RequestHandlers.Product;

public class AddSalesTests : BaseHandlerTest
{
    public AddSalesTests()
    {
        BuildContainer();
    }

    [Fact]
    public async Task AddSales_Success_When_Product_Sale_Is_Completed_Successfully_Test()
    {
        var productToAdd = new Infrastructure.Data.Postgres.Entities.Product { Name = "Test Product" };
        await PostgresContext.Products.AddAsync(productToAdd);

        var supplyToAdd = new Infrastructure.Data.Postgres.Entities.ProductSupply { ProductId = productToAdd.Id, Date = DateTime.UtcNow, Quantity = 100, RemainingQuantity = 100 };
        await PostgresContext.ProductSupplies.AddAsync(supplyToAdd);
        await PostgresContext.SaveChangesAsync();

        var request = new AddSales.AddSalesRequest { Date = DateTime.UtcNow, Quantity = 100 };
        request.GetType().GetProperty(nameof(request.ProductId))?.SetValue(request, productToAdd.Id);
        var response = await Mediator.Send(request);

        await PostgresContext.Entry(supplyToAdd).ReloadAsync();

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal(supplyToAdd.Quantity - 100, supplyToAdd.RemainingQuantity);
    }

    [Fact]
    public async Task AddSales_Fail_When_Specified_Product_Not_Found_Test()
    {
        var request = new AddSales.AddSalesRequest { Date = DateTime.UtcNow, Quantity = 100 };
        request.GetType().GetProperty(nameof(request.ProductId))?.SetValue(request, 1);
        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal(response.Message, "Specified product is not found.");
        Assert.Null(response.Data);
    }


    [Fact]
    public async Task AddSales_Fail_When_Insufficient_Stock_Test()
    {
        var productToAdd = new Infrastructure.Data.Postgres.Entities.Product { Name = "Test Product" };
        await PostgresContext.Products.AddAsync(productToAdd);

        var supplyToAdd = new Infrastructure.Data.Postgres.Entities.ProductSupply { ProductId = productToAdd.Id, Date = DateTime.UtcNow, Quantity = 100, RemainingQuantity = 100 };
        await PostgresContext.ProductSupplies.AddAsync(supplyToAdd);
        await PostgresContext.SaveChangesAsync();

        var request = new AddSales.AddSalesRequest { Date = DateTime.UtcNow, Quantity = 150 };
        request.GetType().GetProperty(nameof(request.ProductId))?.SetValue(request, productToAdd.Id);
        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal(response.Message, "Insufficient stock to complete the sale.");
        Assert.Null(response.Data);
    }

    [Fact]
    public async Task AddSales_Fail_When_Sale_Date_Is_Before_Supply_Date_Test()
    {
        var productToAdd = new Infrastructure.Data.Postgres.Entities.Product { Name = "Test Product" };
        await PostgresContext.Products.AddAsync(productToAdd);

        var supplyToAdd = new Infrastructure.Data.Postgres.Entities.ProductSupply { ProductId = productToAdd.Id, Date = DateTime.UtcNow.AddDays(1), Quantity = 100, RemainingQuantity = 100 };
        await PostgresContext.ProductSupplies.AddAsync(supplyToAdd);
        await PostgresContext.SaveChangesAsync();

        var request = new AddSales.AddSalesRequest { Date = DateTime.UtcNow, Quantity = 150 };
        request.GetType().GetProperty(nameof(request.ProductId))?.SetValue(request, productToAdd.Id);
        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal(response.Message, "Insufficient stock to complete the sale.");
        Assert.Null(response.Data);
    }

    [Fact]
    public async Task AddSales_Validation_Fails_When_Quantity_Is_Less_Than_One_Test()
    {
        var productToAdd = new Infrastructure.Data.Postgres.Entities.Product { Name = "Test Product" };
        await PostgresContext.Products.AddAsync(productToAdd);

        var supplyToAdd = new Infrastructure.Data.Postgres.Entities.ProductSupply { ProductId = productToAdd.Id, Date = DateTime.UtcNow, Quantity = 100, RemainingQuantity = 100 };
        await PostgresContext.ProductSupplies.AddAsync(supplyToAdd);
        await PostgresContext.SaveChangesAsync();

        var request = new AddSales.AddSalesRequest { Date = DateTime.UtcNow, Quantity = 0 };
        request.GetType().GetProperty(nameof(request.ProductId))?.SetValue(request, 1);
        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal(response.Message, "Quantity must be greater than 0.");
        Assert.Null(response.Data);
    }
}
