using Business.RequestHandlers.Organization;
using Shared.Models.Results;

namespace Business.Test.RequestHandlers.Organization;

public class GetPagedOrganizationsTests : BaseHandlerTest
{
    public GetPagedOrganizationsTests()
    {
        BuildContainer();
    }

    [Fact]
    public async Task GetPagedOrganizations_Success_When_NoFilters_Test()
    {
        var organizations = new List<Infrastructure.Data.Postgres.Entities.Organization>
        {
            new() { Id = 1, Name = "Org A", Email = "a@org.com", Phone = "111", Address = "Address 1" },
            new() { Id = 2, Name = "Org B", Email = "b@org.com", Phone = "222", Address = "Address 2" },
            new() { Id = 3, Name = "Org C", Email = "c@org.com", Phone = "333", Address = "Address 3" }
        };

        await PostgresContext.Organizations.AddRangeAsync(organizations);
        await PostgresContext.SaveChangesAsync();

        var request = new GetPagedOrganizations.GetPagedOrganizationsRequest
        {
            PageNumber = 1,
            PageSize = 2
        };

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Equal(2, response.Data.Count());
        Assert.Equal(3, response.TotalCount);
        Assert.Equal(1, response.PageNumber);
        Assert.Equal(2, response.PageSize);
    }

    [Fact]
    public async Task GetPagedOrganizations_Success_With_SearchFilter_Test()
    {
        var organizations = new List<Infrastructure.Data.Postgres.Entities.Organization>
        {
            new() { Id = 1, Name = "Tech Solutions", Email = "tech@org.com", Phone = "111", Address = "Address 1" },
            new() { Id = 2, Name = "Marketing Pros", Email = "marketing@org.com", Phone = "222", Address = "Address 2" },
            new() { Id = 3, Name = "Tech Innovations", Email = "innovations@org.com", Phone = "333", Address = "Address 3" }
        };

        await PostgresContext.Organizations.AddRangeAsync(organizations);
        await PostgresContext.SaveChangesAsync();

        var request = new GetPagedOrganizations.GetPagedOrganizationsRequest
        {
            PageNumber = 1,
            PageSize = 10,
            Search = "Tech"
        };

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Equal(2, response.Data.Count());
        Assert.Equal(2, response.TotalCount);
        Assert.All(response.Data, x => Assert.Contains("Tech", x.Name));
    }

    [Fact]
    public async Task GetPagedOrganizations_Success_When_EmptyResult_Test()
    {
        var request = new GetPagedOrganizations.GetPagedOrganizationsRequest
        {
            PageNumber = 1,
            PageSize = 10,
            Search = "NonExistingOrg"
        };

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Empty(response.Data);
        Assert.Equal(0, response.TotalCount);
    }


    [Fact]
    public async Task GetPagedOrganizations_Fail_When_InvalidPagination_Test()
    {
        var request1 = new GetPagedOrganizations.GetPagedOrganizationsRequest
        {
            PageNumber = 0, // Invalid
            PageSize = 10
        };
        var response1 = await Mediator.Send(request1);
        Assert.Equal(ResultStatus.Invalid, response1.Status);

        var request2 = new GetPagedOrganizations.GetPagedOrganizationsRequest
        {
            PageNumber = 1,
            PageSize = 0 // Invalid
        };
        var response2 = await Mediator.Send(request2);
        Assert.Equal(ResultStatus.Invalid, response2.Status);

        var request3 = new GetPagedOrganizations.GetPagedOrganizationsRequest
        {
            PageNumber = 1,
            PageSize = -1 // Invalid
        };
        var response3 = await Mediator.Send(request3);
        Assert.Equal(ResultStatus.Invalid, response3.Status);
    }

    [Fact]
    public async Task GetPagedOrganizations_Excludes_Deleted_Organizations_Test()
    {
        var organizations = new List<Infrastructure.Data.Postgres.Entities.Organization>
        {
            new() { Id = 1, Name = "Active Org", Email = "active@org.com", Phone = "111", Address = "Address 1" },
            new() { Id = 2, Name = "Deleted Org", Email = "deleted@org.com", Phone = "222", Address = "Address 2", IsDeleted = true }
        };

        await PostgresContext.Organizations.AddRangeAsync(organizations);
        await PostgresContext.SaveChangesAsync();

        var request = new GetPagedOrganizations.GetPagedOrganizationsRequest
        {
            PageNumber = 1,
            PageSize = 10
        };

        var response = await Mediator.Send(request);

        Assert.Equal(ResultStatus.Success, response.Status);
        Assert.Single(response.Data);
        Assert.Equal("Active Org", response.Data.First().Name);
    }

    [Fact]
    public async Task GetPagedOrganizations_Returns_Correct_Response_Structure_Test()
    {
        var organization = new Infrastructure.Data.Postgres.Entities.Organization
        {
            Id = 1,
            Name = "Test Org",
            Email = "test@org.com",
            Phone = "123456789",
            Address = "Test Address"
        };
        await PostgresContext.Organizations.AddAsync(organization);
        await PostgresContext.SaveChangesAsync();

        var request = new GetPagedOrganizations.GetPagedOrganizationsRequest
        {
            PageNumber = 1,
            PageSize = 10
        };

        var response = await Mediator.Send(request);
        
        Assert.Equal(ResultStatus.Success, response.Status);
        var orgResponse = response.Data.First();
        Assert.Equal(organization.Id, orgResponse.Id);
        Assert.Equal(organization.Name, orgResponse.Name);
        Assert.Equal(organization.Email, orgResponse.Email);
        Assert.Equal(organization.Phone, orgResponse.Phone);
        Assert.Equal(organization.Address, orgResponse.Address);
    }
}
