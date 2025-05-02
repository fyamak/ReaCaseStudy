using Business.RequestHandlers.User;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.Results;
using Web.Controllers.Base;
using Web.Filters;

namespace Web.Controllers;

public class UserController(IMediator mediator) : BaseController(mediator)
{
    [HttpPost("/GetProfileInfo")]
    [Authorize]
    public Task<DataResult<GetProfileInfo.GetProfileInfoResponse>> GetProfileInfo(GetProfileInfo.GetProfileInfoRequest request)
    {
        return Mediator.Send(request);
    }

    [HttpPatch]
    [Authorize]
    public async Task<DataResult<string>> EditProfilInfo(EditProfileInfo.EditProfileInfoRequest request)
    {
        return await Mediator.Send(request);
    }

}
