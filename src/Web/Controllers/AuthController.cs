using Business.RequestHandlers.Auth;
using Business.Services.Security.Auth.Jwt.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.Results;
using Web.Controllers.Base;

namespace Web.Controllers;

public class AuthController(IMediator mediator) : BaseController(mediator)
{
    [HttpPost("/login")]
    public async Task<ActionResult<DataResult<Token>>> Login(Login.LoginRequest request)
    {
        return await Mediator.Send(request);
    }

    [HttpPost("/Register")]
    public async Task<ActionResult<DataResult<Token>>> Register(Register.RegisterRequest request)
    {
        return await Mediator.Send(request);
    }

    [HttpPost("/RefreshToken")]
    public async Task<ActionResult<DataResult<Token>>> RefreshToken(RefreshToken.RefreshTokenRequest request)
    {
        return await Mediator.Send(request);
    }

    [HttpPost("/ResetPassword")]
    public async Task<ActionResult<Result>> ResetPassword(ResetPassword.ResetPasswordRequest request)
    {
        return await Mediator.Send(request);
    }
}
