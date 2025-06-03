using Business.RequestHandlers.Order;
using Business.RequestHandlers.Product;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Shared.Models.Results;
using Web.Controllers.Base;
using Web.Filters;

namespace Web.Controllers;

public class ProductController(IMediator mediator) : BaseController(mediator)
{

    [HttpGet("/Products")]
    [Authorize]
    public async Task<DataResult<List<GetAllProducts.GetAllProductsResponse>>> Products ()
    {
        return await Mediator.Send(new GetAllProducts.GetAllProductsRequest());
    }


    [HttpPost("/Products")]
    [Authorize]
    public async Task<DataResult<string>> Products([FromBody] CreateProduct.CreateProductRequest request)
    {
        return await Mediator.Send(request);
    }


    [HttpPut("/Products/{id}")]
    [Authorize]
    public async Task<DataResult<string>> Products (int id, [FromBody] EditProduct.EditProductRequest request)
    {
        request.Id = id;
        return await Mediator.Send(request);
    }

    [HttpPost("/Products/{productId}/supply")]
    [Authorize]
    public async Task<DataResult<string>> AddSupply(int productId, [FromBody] AddSupply.AddSupplyRequest request)
    {
        request.ProductId = productId;
        return await Mediator.Send(request);
    }

    [HttpPost("/Products/{productId}/sale")]
    [Authorize]
    public async Task<DataResult<string>> AddSales(int productId, [FromBody] AddSales.AddSalesRequest request)
    {
        request.ProductId = productId;
        return await Mediator.Send(request);
    }


    [HttpGet("/Transactions/{startDate}/{endDate}")]
    [Authorize]
    public async Task<DataResult<List<Transaction.TransactionResponse>>> Transactions(
        DateTime startDate,
        DateTime endDate,
        [FromQuery] int? productId = null
        )
    {
        return await Mediator.Send(new Transaction.TransactionRequest
        {
            StartDate = startDate,
            EndDate = endDate,
            ProductId = productId
        });
    }


    [HttpGet("/dash")]
    //[Authorize]
    public async Task<DataResult<GetDashboardInformation.GetDashboardInformationResponse>> Dash()
    {
        return await Mediator.Send(new GetDashboardInformation.GetDashboardInformationRequest());
    }


    [HttpGet("/Products/Paged")]
    //[Authorize]
    public async Task<PagedResult<GetPagedProducts.GetPagedProductsResponse>> ProductsPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? categoryId= null)
    {
        return await Mediator.Send(new GetPagedProducts.GetPagedProductsRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search,
            CategoryId = categoryId
        });
    }


    [HttpGet("/Paged/Transactions")]
    //[Authorize]
    public async Task<PagedResult<PagedTransaction.PagedTransactionResponse>> PagedTransaction(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] DateTime startDate = default!,
        [FromQuery] DateTime endDate = default!,
        [FromQuery] bool includeFailures = default!,
        [FromQuery] int? productId = null
        )
    {
        return await Mediator.Send(new PagedTransaction.PagedTransactionRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            StartDate = startDate,
            EndDate = endDate,
            IncludeFailures = includeFailures,
            ProductId = productId
        });
    }
}
