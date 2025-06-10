using Business.RequestHandlers.Category;
using Microsoft.EntityFrameworkCore;
using Shared.Models.Results;

namespace Business.Test.RequestHandlers.Category;

public class CreateCategoryTests : BaseHandlerTest
{
    public CreateCategoryTests()
    {
        BuildContainer();
    }

    [Fact]
    public async Task CreateCategory_Success_When_Request_Is_Valid_Test()
    {
        var request = new CreateCategory.CreateCategoryRequest
        {
            Name = "Electronics"
        };

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.NotNull(response.Data);
        Assert.NotEqual(0, response.Data.Id);
        Assert.Equal(request.Name, response.Data.Name);

        var createdCategory = await PostgresContext.Categories.FirstOrDefaultAsync(c => c.Id == response.Data.Id);
        Assert.NotNull(createdCategory);
        Assert.Equal(request.Name, createdCategory.Name);
    }

    [Fact]
    public async Task CreateCategory_Fail_When_Name_Is_Empty_Test()
    {
        var request = new CreateCategory.CreateCategoryRequest
        {
            Name = string.Empty
        };

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal("Category name must not be empty.", response.Message);
        Assert.Null(response.Data);
    }

    [Fact]
    public async Task CreateCategory_Fail_When_Category_Already_Exists_Test()
    {
        var existingCategory = new Infrastructure.Data.Postgres.Entities.Category
        {
            Name = "Electronics"
        };
        await PostgresContext.Categories.AddAsync(existingCategory);
        await PostgresContext.SaveChangesAsync();

        var request = new CreateCategory.CreateCategoryRequest
        {
            Name = "Electronics" // Same name
        };

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal("Category with same name is already exist", response.Message);
        Assert.Null(response.Data);
    }
}
