using Autofac;
using Business.RequestHandlers.Order;
using Infrastructure.Data.Postgres;
using Infrastructure.Data.Postgres.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using Shared.Models.Results;
using Serilog;

namespace Business.Test.RequestHandlers.Order;

public class GetPagedOrdersTests : BaseHandlerTest
{
    public GetPagedOrdersTests()
    {
        BuildContainer();
    }

    [Fact]
    public async Task GetPagedOrders_Success_When_NoFilters_Test()
    {
        var category = new Infrastructure.Data.Postgres.Entities.Category { Id = 1, Name = "Electronics" };
        var product = new Infrastructure.Data.Postgres.Entities.Product { Id = 1, SKU = "SKU-001", Name = "Test Product", TotalQuantity = 10, CategoryId = category.Id, Category = category };
        var organization = new Infrastructure.Data.Postgres.Entities.Organization { Id = 1, Name = "Org A", Email = "a@org.com", Phone = "111", Address = "Address 1" };
        var orders = new List<Infrastructure.Data.Postgres.Entities.Order>
        {
            new() {
                Id = 1,
                ProductId = 1,
                OrganizationId = 1,
                Quantity = 5,
                Price = 99.99,
                Date = DateTime.UtcNow.AddDays(-1),
                Type = "supply"
            },
            new() {
                Id = 2,
                ProductId = 1,
                OrganizationId = 1,
                Quantity = 10,
                Price = 199.99,
                Date = DateTime.UtcNow,
                Type = "sale"
            }
        };

        await PostgresContext.Categories.AddAsync(category);
        await PostgresContext.Products.AddAsync(product);
        await PostgresContext.Organizations.AddAsync(organization);
        await PostgresContext.Orders.AddRangeAsync(orders);
        await PostgresContext.SaveChangesAsync();

        var request = new GetPagedOrders.GetPagedOrdersRequest
        {
            PageNumber = 1,
            PageSize = 10
        };

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Equal(2, response.Data.Count());
        Assert.Equal(2, response.TotalCount);
        Assert.Equal(1, response.PageNumber);
        Assert.Equal(10, response.PageSize);
    }

    [Fact]
    public async Task GetPagedOrders_Success_With_TypeFilter_Test()
    {
        var product = new Infrastructure.Data.Postgres.Entities.Product { Id = 1, SKU = "SKU-001", Name = "Test Product", TotalQuantity = 0, CategoryId = 1 };
        var organization = new Infrastructure.Data.Postgres.Entities.Organization { Id = 1, Name = "Org A", Email = "a@org.com", Phone = "111", Address = "Address 1" };
        var orders = new List<Infrastructure.Data.Postgres.Entities.Order>
        {
            new() { Id = 1, ProductId = 1, OrganizationId = 1, Quantity = 1, Price = 1, Date = DateTime.UtcNow, Type = "supply" },
            new() { Id = 2, ProductId = 1, OrganizationId = 1, Quantity = 1, Price = 1, Date = DateTime.UtcNow, Type = "sale" },
            new() { Id = 3, ProductId = 1, OrganizationId = 1, Quantity = 1, Price = 1, Date = DateTime.UtcNow, Type = "supply" },
        };

        await PostgresContext.Products.AddAsync(product);
        await PostgresContext.Organizations.AddAsync(organization);
        await PostgresContext.Orders.AddRangeAsync(orders);
        await PostgresContext.SaveChangesAsync();

        var request = new GetPagedOrders.GetPagedOrdersRequest
        {
            PageNumber = 1,
            PageSize = 10,
            Type = "supply"
        };

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Equal(2, response.Data.Count());
        Assert.All(response.Data, x => Assert.Equal("supply", x.Type));
    }

    [Fact]
    public async Task GetPagedOrders_Success_With_SearchFilter_Test()
    {
        var product = new Infrastructure.Data.Postgres.Entities.Product { Id = 1, SKU = "SKU-001", Name = "Test Product", TotalQuantity = 0, CategoryId = 1 };
        var organization = new Infrastructure.Data.Postgres.Entities.Organization { Id = 1, Name = "Org A", Email = "a@org.com", Phone = "111", Address = "Address 1" };
        var orders = new List<Infrastructure.Data.Postgres.Entities.Order>
        {
            new() { Id = 1, ProductId = 1, OrganizationId = 1, Quantity = 1, Price = 1, Date = DateTime.UtcNow, Type = "supply" },
            new() { Id = 2, ProductId = 1, OrganizationId = 1, Quantity = 1, Price = 1, Date = DateTime.UtcNow, Type = "sale" },
            new() { Id = 3, ProductId = 1, OrganizationId = 1, Quantity = 1, Price = 1, Date = DateTime.UtcNow, Type = "supply" },
        };

        await PostgresContext.Products.AddAsync(product);
        await PostgresContext.Organizations.AddAsync(organization);
        await PostgresContext.Orders.AddRangeAsync(orders);
        await PostgresContext.SaveChangesAsync();


        var request1 = new GetPagedOrders.GetPagedOrdersRequest
        {
            PageNumber = 1,
            PageSize = 10,
            Search = "Test Product"
        };

        var response1 = await Mediator.Send(request1);

        Assert.Equal(ResultStatus.Success, response1.Status);
        Assert.Equal(3, response1.Data.Count());


        var request2 = new GetPagedOrders.GetPagedOrdersRequest
        {
            PageNumber = 1,
            PageSize = 10,
            Search = "Org A"
        };

        var response2 = await Mediator.Send(request2);

        Assert.Equal(ResultStatus.Success, response2.Status);
        Assert.Equal(3, response2.Data.Count());


    }

    [Fact]
    public async Task GetPagedOrders_Success_With_IsDeleted_Filter_Test()
    {
        var product = new Infrastructure.Data.Postgres.Entities.Product { Id = 1, SKU = "SKU-001", Name = "Test Product", TotalQuantity = 0, CategoryId = 1 };
        var organization = new Infrastructure.Data.Postgres.Entities.Organization { Id = 1, Name = "Org A", Email = "a@org.com", Phone = "111", Address = "Address 1" };
        var orders = new List<Infrastructure.Data.Postgres.Entities.Order>
        {
            new() { Id = 1, ProductId = 1, OrganizationId = 1, Quantity = 1, Price = 1, Date = DateTime.UtcNow, Type = "supply", IsDeleted = false },
            new() { Id = 2, ProductId = 1, OrganizationId = 1, Quantity = 1, Price = 1, Date = DateTime.UtcNow, Type = "sale", IsDeleted = true },
        };

        await PostgresContext.Products.AddAsync(product);
        await PostgresContext.Organizations.AddAsync(organization);
        await PostgresContext.Orders.AddRangeAsync(orders);
        await PostgresContext.SaveChangesAsync();

        var request1 = new GetPagedOrders.GetPagedOrdersRequest
        {
            PageNumber = 1,
            PageSize = 10,
            IsDeleted = true
        };
        var response1 = await Mediator.Send(request1);
        Assert.Equal(ResultStatus.Success, response1.Status);
        Assert.Single(response1.Data);

        var request2 = new GetPagedOrders.GetPagedOrdersRequest
        {
            PageNumber = 1,
            PageSize = 10,
            IsDeleted = false // (default)
        };
        var response2 = await Mediator.Send(request2);
        Assert.Equal(ResultStatus.Success, response2.Status);
        Assert.Single(response2.Data);
    }

    [Fact]
    public async Task GetPagedOrders_Fail_When_InvalidPagination_Test()
    {
        var request1 = new GetPagedOrders.GetPagedOrdersRequest
        {
            PageNumber = 0, // Invalid
            PageSize = 10
        };
        var response1 = await Mediator.Send(request1);
        Assert.Equal(ResultStatus.Invalid, response1.Status);

        var request2 = new GetPagedOrders.GetPagedOrdersRequest
        {
            PageNumber = 1,
            PageSize = 0 // Invalid
        };
        var response2 = await Mediator.Send(request2);
        Assert.Equal(ResultStatus.Invalid, response2.Status);
    }
}
