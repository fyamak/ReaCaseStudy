using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Data.Postgres.Entities;
using Business.RequestHandlers.Product;
using Shared.Models.Results;

namespace Business.Test.RequestHandlers.Product;

public class CreateProductTests: BaseHandlerTest
{
    public CreateProductTests()
    {
        BuildContainer();
    }

    [Fact]
    public async Task CreateProduct_Success_When_Product_Is_Created_Successfully_Test()
    {
        var request = new CreateProduct.CreateProductRequest { Name = "Valid Test" };
        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.NotNull(response.Data);
    }


    [Fact]
    public async Task CreateProduct_Fail_When_Product_Name_Is_Empty_Test()
    {
        var request = new CreateProduct.CreateProductRequest { Name = null };
        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
    }


    [Fact]
    public async Task CreateProduct_Fail_When_Product_Name_Is_Too_Short_Test()
    {
        var request = new CreateProduct.CreateProductRequest { Name = null };
        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
    }


    [Fact]
    public async Task CreateProduct_Fail_When_Product_With_Same_Name_Already_Exists_Test()
    {
        var productToAdd = new Infrastructure.Data.Postgres.Entities.Product { Name = "Test Product" };
        await PostgresContext.Products.AddAsync(productToAdd);
        await PostgresContext.SaveChangesAsync();

        var request = new CreateProduct.CreateProductRequest { Name = "Test Product" };
        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Null(response.Data);
    }


}
