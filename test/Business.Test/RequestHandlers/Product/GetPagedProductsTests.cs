using Business.RequestHandlers.Product;
using Shared.Models.Results;

namespace Business.Test.RequestHandlers.Product;

public class GetPagedProductsTests : BaseHandlerTest
{
    public GetPagedProductsTests()
    {
        BuildContainer();
    }

    [Fact]
    public async Task GetPagedProducts_Success_When_NoFilters_Test()
    {
        const int PAGE_NUMBER = 1;
        const int PAGE_SIZE = 2;
        var category = new Infrastructure.Data.Postgres.Entities.Category { Id = 1, Name = "Electronics" };
        var products = new List<Infrastructure.Data.Postgres.Entities.Product>
        {
            new() { Id = 1, SKU = "SKU-001", Name = "Laptop", TotalQuantity = 10, CategoryId = category.Id, Category = category },
            new() { Id = 2, SKU = "SKU-002", Name = "Smartphone", TotalQuantity = 20, CategoryId = category.Id, Category = category },
            new() { Id = 3, SKU = "SKU-003", Name = "Tablet", TotalQuantity = 15, CategoryId = category.Id, Category = category }
        };

        await PostgresContext.Categories.AddAsync(category);
        await PostgresContext.Products.AddRangeAsync(products);
        await PostgresContext.SaveChangesAsync();

        var request = new GetPagedProducts.GetPagedProductsRequest
        {
            PageNumber = PAGE_NUMBER,
            PageSize = PAGE_SIZE
        };

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Equal(2, response.Data.Count()); // PageSize = 2
        Assert.Equal(3, response.TotalCount); // Total items
        Assert.Equal(PAGE_NUMBER, response.PageNumber);
        Assert.Equal(PAGE_SIZE, response.PageSize);
    }


    [Fact]
    public async Task GetPagedProducts_Success_With_SearchFilter_Test()
    {
        var category = new Infrastructure.Data.Postgres.Entities.Category { Id = 1, Name = "Electronics" };
        var products = new List<Infrastructure.Data.Postgres.Entities.Product>
        {
            new() { Id = 1, SKU = "SKU-001", Name = "Laptop", TotalQuantity = 10, CategoryId = category.Id, Category = category },
            new() { Id = 2, SKU = "SKU-002", Name = "Smartphone", TotalQuantity = 20, CategoryId = category.Id, Category = category },
            new() { Id = 3, SKU = "SKU-003", Name = "Tablet", TotalQuantity = 15, CategoryId = category.Id, Category = category }
        };

        await PostgresContext.Categories.AddAsync(category);
        await PostgresContext.Products.AddRangeAsync(products);
        await PostgresContext.SaveChangesAsync();

        var request = new GetPagedProducts.GetPagedProductsRequest
        {
            PageNumber = 1,
            PageSize = 10,
            Search = "phone"
        };

        var response = await Mediator.Send(request);
        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Equal("Smartphone", response.Data.First().Name);
    }


    [Fact]
    public async Task GetPagedProducts_Success_With_CategoryFilter_Test()
    {
        var electronics = new Infrastructure.Data.Postgres.Entities.Category { Id = 1, Name = "Electronics" };
        var clothing = new Infrastructure.Data.Postgres.Entities.Category { Id = 2, Name = "Clothing" };
        var products = new List<Infrastructure.Data.Postgres.Entities.Product>
        {
            new() { Id = 1, SKU = "SKU-001", Name = "Laptop", TotalQuantity = 10, CategoryId = 1, Category = electronics },
            new() { Id = 2, SKU = "SKU-002", Name = "T-Shirt", TotalQuantity = 20, CategoryId = 2, Category = clothing }
        };

        await PostgresContext.Categories.AddRangeAsync(electronics, clothing);
        await PostgresContext.Products.AddRangeAsync(products);
        await PostgresContext.SaveChangesAsync();

        var request = new GetPagedProducts.GetPagedProductsRequest
        {
            PageNumber = 1,
            PageSize = 10,
            CategoryId = 2 // Clothing
        };

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Single(response.Data);
        Assert.Equal("T-Shirt", response.Data.First().Name);
        Assert.Equal("Clothing", response.Data.First().CategoryName);
    }


    [Fact]
    public async Task GetPagedProducts_Success_When_EmptyResult_Test()
    {
        var request = new GetPagedProducts.GetPagedProductsRequest
        {
            PageNumber = 1,
            PageSize = 10,
        };

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Empty(response.Data);
        Assert.Equal(0, response.TotalCount);
    }


    [Fact]
    public async Task GetPagedProducts_Fail_When_InvalidPagination_Test()
    {
        // Invalid PageNumber -> 0
        var request1 = new GetPagedProducts.GetPagedProductsRequest
        {
            PageNumber = 0,
            PageSize = 10
        };
        var response1 = await Mediator.Send(request1);
        Assert.Equal(ResultStatus.Invalid, response1.Status);

        // Invalid PageSize -> 0
        var request2 = new GetPagedProducts.GetPagedProductsRequest
        {
            PageNumber = 1,
            PageSize = 0
        };
        var response2 = await Mediator.Send(request2);
        Assert.Equal(ResultStatus.Invalid, response2.Status);

        // Invalid PageSize -> Negative
        var request3 = new GetPagedProducts.GetPagedProductsRequest
        {
            PageNumber = 1,
            PageSize = -1
        };
        var response3 = await Mediator.Send(request3);
        Assert.Equal(ResultStatus.Invalid, response3.Status);
    }
}
