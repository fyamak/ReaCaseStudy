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
    public Task<DataResult<List<GetAllProducts.GetAllProductsResponse>>> Products ()
    {
        return Mediator.Send(new GetAllProducts.GetAllProductsRequest());
    }


    [HttpPost("/Products")]
    [Authorize]
    public Task<DataResult<CreateProduct.CreateProductResponse>> Products(CreateProduct.CreateProductRequest request)
    {
        return Mediator.Send(request);
    }


    // use update instead of edit
    [HttpPut("/Products/{id}")]
    [Authorize]
    public Task<DataResult<EditProduct.EditProductResponse>> Products (int id, EditProduct.EditProductRequest request)
    {
        request.GetType().GetProperty(nameof(request.Id))?.SetValue(request, id);
        return Mediator.Send(request);
    }

    [HttpPost("/Products/{productId}/Supplies")]
    [Authorize]
    public Task<DataResult<AddSupply.AddSupplyResponse>> AddSupply(int productId,AddSupply.AddSupplyRequest request)
    {
        request.GetType().GetProperty(nameof(request.ProductId))?.SetValue(request, productId);
        return Mediator.Send(request);
    }

    [HttpPost("/Products/{productId}/Sales")]
    [Authorize]
    public Task<DataResult<List<AddSales.AddSalesResponse>>> AddSales(int productId, AddSales.AddSalesRequest request)
    {
        request.GetType().GetProperty(nameof(request.ProductId))?.SetValue(request, productId);
        return Mediator.Send(request);
    }



    [HttpGet("/Transactions/{startDate}/{endDate}")]
    [Authorize]
    public Task<DataResult<List<Transaction.TransactionResponse>>> Transactions(
        DateTime startDate,
        DateTime endDate,
        [FromQuery] int? productId = null
        )
    {
        return Mediator.Send(new Transaction.TransactionRequest
        {
            StartDate = startDate,
            EndDate = endDate,
            ProductId = productId
        });
    }
}
