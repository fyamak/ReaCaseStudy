using Business.RequestHandlers.Category;
using Shared.Models.Results;

namespace Business.Test.RequestHandlers.Category;

public class DeleteCategoryTests : BaseHandlerTest
{
    public DeleteCategoryTests()
    {
        BuildContainer();
    }

    [Fact]
    public async Task DeleteCategory_Success_When_Category_Exists_Test()
    {
        var category = new Infrastructure.Data.Postgres.Entities.Category { Id = 1, Name = "Electronics" };
        await PostgresContext.Categories.AddAsync(category);
        await PostgresContext.SaveChangesAsync();

        var request = new DeleteCategory.DeleteCategoryRequest { Id = 1 };

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Equal($"Category with id {category.Id} is deleted successfully", response.Data);
    }

    [Fact]
    public async Task DeleteCategory_Fail_When_Category_Not_Found_Test()
    {
        var request = new DeleteCategory.DeleteCategoryRequest { Id = 9 };

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal("Invalid category Id", response.Message);
    }

    [Fact]
    public async Task DeleteCategory_Fail_When_Category_Already_Deleted_Test()
    {
        var category = new Infrastructure.Data.Postgres.Entities.Category { Id = 1, Name = "Electronics", IsDeleted = true };
        await PostgresContext.Categories.AddAsync(category);
        await PostgresContext.SaveChangesAsync();

        var request = new DeleteCategory.DeleteCategoryRequest { Id = 1 };

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal("Invalid category Id", response.Message);
    }
}
