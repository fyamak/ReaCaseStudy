using Business.RequestHandlers.Category;
using Shared.Models.Results;

namespace Business.Test.RequestHandlers.Category;

public class GetPagedCategoriesTests : BaseHandlerTest
{
    public GetPagedCategoriesTests()
    {
        BuildContainer();
    }

    [Fact]
    public async Task GetPagedCategories_Success_When_NoFilters_Test()
    {
        var categories = new List<Infrastructure.Data.Postgres.Entities.Category>
        {
            new() { Id = 1, Name = "Electronics" },
            new() { Id = 2, Name = "Clothing" },
            new() { Id = 3, Name = "Home Appliances" }
        };

        await PostgresContext.Categories.AddRangeAsync(categories);
        await PostgresContext.SaveChangesAsync();

        var request = new GetPagedCategories.GetPagedCategoriesRequest
        {
            PageNumber = 1,
            PageSize = 2
        };

        var response = await Mediator.Send(request);
        
        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Equal(2, response.Data.Count()); // PageSize = 2
        Assert.Equal(3, response.TotalCount);
        Assert.Equal(1, response.PageNumber);
        Assert.Equal(2, response.PageSize);
    }

    [Fact]
    public async Task GetPagedCategories_Success_With_SearchFilter_Test()
    {
        var categories = new List<Infrastructure.Data.Postgres.Entities.Category>
        {
            new() { Id = 1, Name = "Electronics" },
            new() { Id = 2, Name = "Clothing" },
            new() { Id = 3, Name = "Home Electronics" }
        };

        await PostgresContext.Categories.AddRangeAsync(categories);
        await PostgresContext.SaveChangesAsync();

        var request = new GetPagedCategories.GetPagedCategoriesRequest
        {
            PageNumber = 1,
            PageSize = 10,
            Search = "Electronics"
        };

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Equal(2, response.Data.Count());
        Assert.Equal(2, response.TotalCount);
        Assert.All(response.Data, x => Assert.Contains("Electronics", x.Name));
        Assert.Equal(1, response.PageNumber);
        Assert.Equal(10, response.PageSize);
    }

    [Fact]
    public async Task GetPagedCategories_Success_When_EmptyResult_Test()
    {
        var request = new GetPagedCategories.GetPagedCategoriesRequest
        {
            PageNumber = 1,
            PageSize = 10,
            Search = "NonExistingCategory"
        };

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Empty(response.Data);
        Assert.Equal(0, response.TotalCount);
    }

    [Fact]
    public async Task GetPagedCategories_Fail_When_InvalidPagination_Test()
    {
        var request1 = new GetPagedCategories.GetPagedCategoriesRequest
        {
            PageNumber = 0, // Invalid
            PageSize = 10
        };
        var response1 = await Mediator.Send(request1);
        Assert.Equal(ResultStatus.Invalid, response1.Status);

        var request2 = new GetPagedCategories.GetPagedCategoriesRequest
        {
            PageNumber = 1,
            PageSize = 0 // Invalid
        };
        var response2 = await Mediator.Send(request2);
        Assert.Equal(ResultStatus.Invalid, response2.Status);

        var request3 = new GetPagedCategories.GetPagedCategoriesRequest
        {
            PageNumber = 1,
            PageSize = -1 // Invalid
        };
        var response3 = await Mediator.Send(request3);
        Assert.Equal(ResultStatus.Invalid, response3.Status);
    }

    [Fact]
    public async Task GetPagedCategories_Excludes_Deleted_Categories_Test()
    {
        var categories = new List<Infrastructure.Data.Postgres.Entities.Category>
        {
            new() { Id = 1, Name = "Active Category" },
            new() { Id = 2, Name = "Deleted Category", IsDeleted = true }
        };

        await PostgresContext.Categories.AddRangeAsync(categories);
        await PostgresContext.SaveChangesAsync();

        var request = new GetPagedCategories.GetPagedCategoriesRequest
        {
            PageNumber = 1,
            PageSize = 10
        };

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Single(response.Data);
        Assert.Equal("Active Category", response.Data.First().Name);
    }
}
