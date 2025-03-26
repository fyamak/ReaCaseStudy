//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Business.RequestHandlers.Product;
//using Shared.Models.Results;

//namespace Business.Test.RequestHandlers.Product;

//public class AddSupplyTests: BaseHandlerTest
//{
//    public AddSupplyTests()
//    {
//        BuildContainer();
//    }

    
//    [Fact]
//    public async Task AddSupply_Success_When_Product_Supply_Is_Added_Successfully_Test()
//    {
//        var productToAdd = new Infrastructure.Data.Postgres.Entities.Product { Name = "Test Product" };
//        await PostgresContext.Products.AddAsync(productToAdd);
//        await PostgresContext.SaveChangesAsync();

//        var request = new AddSupply.AddSupplyRequest { Date = DateTime.UtcNow, Quantity = 100 };
//        request.GetType().GetProperty(nameof(request.ProductId))?.SetValue(request, productToAdd.Id);
//        var response = await Mediator.Send(request);

//        Assert.Equal(ResultStatus.Success, response.Status);
//        Assert.Equal(response.Data.Quantity, response.Data.RemainingQuantity);
//        Assert.NotNull(response.Data);
//    }

//    [Fact]
//    public async Task AddSupply_Fail_When_Specified_Product_Not_Found_Test()
//    {
//        var request = new AddSupply.AddSupplyRequest { Date = DateTime.UtcNow, Quantity = 100 };
//        request.GetType().GetProperty(nameof(request.ProductId))?.SetValue(request, 1);
//        var response = await Mediator.Send(request);

//        Assert.Equal(ResultStatus.Invalid, response.Status);
//        Assert.Equal(response.Message, "Specified product is not found.");
//        Assert.Null(response.Data);
//    }


//    [Fact]
//    public async Task AddSupply_Fail_When_Request_Is_Not_Valid_Test()
//    {
//        var productToAdd = new Infrastructure.Data.Postgres.Entities.Product { Name = "Test Product" };
//        await PostgresContext.Products.AddAsync(productToAdd);
//        await PostgresContext.SaveChangesAsync();

//        var request1 = new AddSupply.AddSupplyRequest { Date = DateTime.UtcNow};
//        request1.GetType().GetProperty(nameof(request1.ProductId))?.SetValue(request1, productToAdd.Id);
//        var response1 = await Mediator.Send(request1);

//        Assert.Equal(ResultStatus.Invalid, response1.Status);
//        Assert.Null(response1.Data);

//        var request2 = new AddSupply.AddSupplyRequest { Quantity = 100 };
//        request2.GetType().GetProperty(nameof(request2.ProductId))?.SetValue(request2, productToAdd.Id);
//        var response2 = await Mediator.Send(request2);

//        Assert.Equal(ResultStatus.Invalid, response2.Status);
//        Assert.Null(response2.Data);
//    }


//    [Fact]
//    public async Task AddSupply_Validation_Fails_When_Quantity_Is_Less_Than_One_Test()
//    {
//        var productToAdd = new Infrastructure.Data.Postgres.Entities.Product { Name = "Test Product" };
//        await PostgresContext.Products.AddAsync(productToAdd);
//        await PostgresContext.SaveChangesAsync();

//        var request = new AddSupply.AddSupplyRequest { Date = DateTime.UtcNow, Quantity = 0 };
//        request.GetType().GetProperty(nameof(request.ProductId))?.SetValue(request, productToAdd.Id);
//        var response = await Mediator.Send(request);

//        Assert.Equal(ResultStatus.Invalid, response.Status);
//        Assert.Equal(response.Message, "Quantity must be greater than 0.");
//        Assert.Null(response.Data);
//    }
//}
