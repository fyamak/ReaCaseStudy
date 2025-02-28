using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers.Base;

[ApiController]
[Route("api/[controller]")]
public class BaseController : ControllerBase
{
    protected readonly IMediator Mediator;

    public BaseController(IMediator mediator)
    {
        Mediator = mediator;
    }
}
