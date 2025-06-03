using Business.RequestHandlers.Order;
using Business.RequestHandlers.Product;
using Infrastructure.Data.Postgres.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.Results;
using Web.Controllers.Base;
using Web.Filters;

namespace Web.Controllers;

public class OrderController(IMediator mediator) : BaseController(mediator)
{
    [HttpGet("/orders")]
    [Authorize]
    public async Task<DataResult<List<GetAllOrders.GetAllOrdersResponse>>> Orders([FromQuery] bool? isDeleted = null)
    {
        return await Mediator.Send(new GetAllOrders.GetAllOrdersRequest { IsDeleted = isDeleted});
    }

    [HttpPost("/orders")]
    [Authorize]
    public async Task<DataResult<string>> Orders([FromBody] CreateOrder.CreateOrderRequest request)
    {
        return await Mediator.Send(request);
    }

    [HttpDelete("/orders/{id}")]
    [Authorize]
    public async Task<DataResult<string>> Orders(int id)
    {
        return await Mediator.Send(new DeleteOrder.DeleteOrderRequest { Id = id });
    }

    [HttpGet("Paged")]
    //[Authorize]
    public async Task<PagedResult<GetPagedOrders.GetPagedOrdersResponse>> PagedOrders(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool isDeleted = false,
        [FromQuery] string? search = null,
        [FromQuery] string? type = null)
    {
        return await Mediator.Send(new GetPagedOrders.GetPagedOrdersRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            IsDeleted = isDeleted,
            Search = search,
            Type = type
        });
    }
}
