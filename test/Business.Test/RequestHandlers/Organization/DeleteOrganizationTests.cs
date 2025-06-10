using Business.RequestHandlers.Organization;
using Shared.Models.Results;

namespace Business.Test.RequestHandlers.Organization;

public class DeleteOrganizationTests : BaseHandlerTest
{
    public DeleteOrganizationTests()
    {
        BuildContainer();
    }

    [Fact]
    public async Task DeleteOrganization_Success_When_Organization_Exists_Test()
    {
        var organization = new Infrastructure.Data.Postgres.Entities.Organization { Id = 1, Name = "Test Org", Email = "test@example.com", Phone = "(555) 555-5555", Address = "Address Example" };
        await PostgresContext.Organizations.AddAsync(organization);
        await PostgresContext.SaveChangesAsync();

        var request = new DeleteOrganization.DeleteOrganizationRequest { Id = 1 };

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Equal($"Organization with id {organization.Id} is deleted successfully", response.Data);
    }

    [Fact]
    public async Task DeleteOrganization_Fail_When_Organization_Not_Found_Test()
    {
        var request = new DeleteOrganization.DeleteOrganizationRequest { Id = 9 };

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal("Invalid organization Id", response.Message);
    }

    [Fact]
    public async Task DeleteOrganization_Fail_When_Organization_Already_Deleted_Test()
    {
        var organization = new Infrastructure.Data.Postgres.Entities.Organization { Id = 1, Name = "Test Org", Email = "test@example.com", Phone = "(555) 555-5555", Address = "Address Example", IsDeleted = true };

        await PostgresContext.Organizations.AddAsync(organization);
        await PostgresContext.SaveChangesAsync();

        var request = new DeleteOrganization.DeleteOrganizationRequest { Id = 1 };

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Invalid, response.Status);
        Assert.Equal("Invalid organization Id", response.Message);
    }
}
