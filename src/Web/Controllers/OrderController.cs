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
    public async Task<DataResult<List<GetAllOrders.GetAllOrdersResponse>>> Orders()
    {
        return await Mediator.Send(new GetAllOrders.GetAllOrdersRequest());
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
}
