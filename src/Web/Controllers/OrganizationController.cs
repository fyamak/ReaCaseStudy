using Business.RequestHandlers.Organization;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.Results;
using Web.Controllers.Base;
using Web.Filters;

namespace Web.Controllers;

public class OrganizationController(IMediator mediator) : BaseController(mediator)
{

    [HttpGet]
    [Authorize]
    public async Task<DataResult<List<GetAllOrganizations.GetAllOrganizationsResponse>>> Organizations()
    {
        return await Mediator.Send(new GetAllOrganizations.GetAllOrganizationsRequest());
    }

    [HttpPost]
    [Authorize]
    public async Task<DataResult<string>> Organizations(CreateOrganization.CreateOrganizationRequest request)
    {
        return await Mediator.Send(request);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<DataResult<string>> Organizations(int id)
    {
        return await Mediator.Send(new DeleteOrganization.DeleteOrganizationRequest { Id = id});
    }

    [HttpGet("Paged")]
    //[Authorize]
    public async Task<PagedResult<GetPagedOrganizations.GetPagedOrganizationsResponse>> OrganizationsPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        return await Mediator.Send(new GetPagedOrganizations.GetPagedOrganizationsRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search
        });
    }

}
