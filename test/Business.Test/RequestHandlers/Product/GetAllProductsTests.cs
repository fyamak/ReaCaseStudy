using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.RequestHandlers.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Shared.Models.Results;

namespace Business.Test.RequestHandlers.Product;

public class GetAllProductsTests : BaseHandlerTest
{
    public GetAllProductsTests()
    {
        BuildContainer();
    }

    [Fact]
    public async Task GetAllProducts_Success_When_Products_Are_Found_Test()
    {
        var productToAdd1 = new Infrastructure.Data.Postgres.Entities.Product { Name = "Test Product 1" };
        await PostgresContext.Products.AddAsync(productToAdd1);
        var productToAdd2 = new Infrastructure.Data.Postgres.Entities.Product { Name = "Test Product 2" };
        await PostgresContext.Products.AddAsync(productToAdd2);
        await PostgresContext.SaveChangesAsync();

        var response = await Mediator.Send(new GetAllProducts.GetAllProductsRequest());

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.NotNull(response.Data);
        Assert.Equal(response.Data.Count, 2);
    }

    [Fact]
    public async Task GetAllProducts_Fail_When_No_Products_Are_Found_Test()
    {
        var response = await Mediator.Send(new GetAllProducts.GetAllProductsRequest());

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal(response.Message, "No products found.");
        Assert.Null(response.Data);
    }
}
