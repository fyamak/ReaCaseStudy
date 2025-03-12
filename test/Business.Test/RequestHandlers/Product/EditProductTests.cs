using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Business.RequestHandlers.Product;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using Shared.Models.Results;

namespace Business.Test.RequestHandlers.Product;

public class EditProductTests : BaseHandlerTest
{
    public EditProductTests()
    {
        BuildContainer();
    }

    [Fact]
    public async Task EditProduct_Success_When_Product_Is_Updated_Successfully_Test()
    {
        var productToAdd = new Infrastructure.Data.Postgres.Entities.Product { Name = "Test Product" };
        await PostgresContext.Products.AddAsync(productToAdd);
        await PostgresContext.SaveChangesAsync();

        var request = new EditProduct.EditProductRequest { Name = "Updated Test Product" };
        request.GetType().GetProperty(nameof(request.Id))?.SetValue(request, productToAdd.Id);
        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.NotNull(response.Data.UpdatedAt);
    }

    [Fact]
    public async Task EditProduct_Fail_When_Product_Id_Is_Invalid_Test()
    {
        var request = new EditProduct.EditProductRequest { Name = "Updated Test Product" };
        request.GetType().GetProperty(nameof(request.Id))?.SetValue(request, 1);
        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal(response.Message, "Invalid product id.");
        Assert.Null(response.Data);
    }

    [Fact]
    public async Task EditProduct_Fail_When_Request_Is_Not_Valid_Test()
    {
        var productToAdd = new Infrastructure.Data.Postgres.Entities.Product { Name = "Test Product" };
        await PostgresContext.Products.AddAsync(productToAdd);
        await PostgresContext.SaveChangesAsync();

        var request = new EditProduct.EditProductRequest { };
        request.GetType().GetProperty(nameof(request.Id))?.SetValue(request, productToAdd.Id);
        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal(response.Message, "Name cannot be empty.");
        Assert.Null(response.Data);
    }

    [Fact]
    public async Task EditProduct_Fail_When_Product_With_Same_Name_Already_Exists_Test()
    {
        var productToAdd = new Infrastructure.Data.Postgres.Entities.Product { Name = "Test Product" };
        await PostgresContext.Products.AddAsync(productToAdd);
        await PostgresContext.SaveChangesAsync();

        var request = new EditProduct.EditProductRequest { Name = "Test Product" };
        request.GetType().GetProperty(nameof(request.Id))?.SetValue(request, productToAdd.Id);
        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal(response.Message, "Product with same name already exists");
        Assert.Null(response.Data);
    }
}
