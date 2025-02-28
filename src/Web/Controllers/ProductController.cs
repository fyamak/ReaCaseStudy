using Business.RequestHandlers.Product;
using Business.RequestHandlers.User;
using Infrastructure.Data.Postgres.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using Shared.Models.Results;
using Web.Controllers.Base;
using Web.Filters;


namespace Web.Controllers;

public class ProductController(IMediator mediator) : BaseController(mediator)
{


    [HttpGet("/Products")]
    //[Authorize]
    public Task<DataResult<List<GetAllProducts.GetAllProductsResponse>>> Products ()
    {
        return Mediator.Send(new GetAllProducts.GetAllProductsRequest());
    }


    [HttpPost("/Products")]
    //[Authorize]
    public Task<DataResult<CreateProduct.CreateProductResponse>> Products(CreateProduct.CreateProductRequest request)
    {
        return Mediator.Send(request);
    }


    // use update instead of edit
    [HttpPut("/Products/{id}")]
    //[Authorize]
    public Task<DataResult<EditProduct.EditProductResponse>> Products (int id, EditProduct.EditProductRequest request)
    {
        //request.Id = id;
        request.GetType().GetProperty(nameof(request.Id))?.SetValue(request, id);
        return Mediator.Send(request);
    }

    [HttpPost("/Products/{productId}/Supplies")]
    //[Authorize]
    public Task<DataResult<AddSupply.AddSupplyResponse>> AddSupply(int productId,AddSupply.AddSupplyRequest request)
    {
        request.GetType().GetProperty(nameof(request.ProductId))?.SetValue(request, productId);
        return Mediator.Send(request);
    }

    [HttpPost("/Products/{productId}/Sales")]
    //[Authorize]
    public Task<DataResult<List<AddSales.AddSalesResponse>>> AddSales(int productId, AddSales.AddSalesRequest request)
    {
        request.GetType().GetProperty(nameof(request.ProductId))?.SetValue(request, productId);
        return Mediator.Send(request);
    }



    //[Authorize]
    //[HttpGet("/Transactions")]
    [HttpGet("/Transactions/{startDate}/{endDate}")]
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
