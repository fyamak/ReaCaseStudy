using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Infrastructure.Data.Postgres;
using Infrastructure.Data.Postgres.Entities;
using MediatR;
using Serilog;
using Serilog.Events;
using Shared.Extensions;
using Shared.Models.Results;
using static Business.RequestHandlers.Product.AddSupply;

namespace Business.RequestHandlers.Product
{
    public class AddSales
    {
        public class AddSalesRequest : IRequest<DataResult<List<AddSalesResponse>>>
        {
            public int ProductId;
            public int Quantity { get; set; }
            public DateTime Date { get; set; }
        }
        public class AddSalesResponse
        {
            public int Id{ get; set; }
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public DateTime Date { get; set; }
            public int RemainingQuantity { get; set; }
        }

        public class AddSalesRequestValidator : AbstractValidator<AddSalesRequest>
        {
            public AddSalesRequestValidator()
            {
                RuleFor(x => x.Quantity)
                 .GreaterThanOrEqualTo(1)
                 .WithMessage("Quantity must be greater than 0.");

                RuleFor(x => x.ProductId)
                    .NotEmpty()
                    .WithMessage("Product id cannot be empty.");

                RuleFor(x => x.Quantity)
                    .NotEmpty()
                    .WithMessage("Quantity cannot be empty.");

                RuleFor(x => x.Date)
                    .NotEmpty()
                    .WithMessage("Date cannot be empty.");
            }
        }

        public class AddSalesRequestHandler : IRequestHandler<AddSalesRequest, DataResult<List<AddSalesResponse>>>
        {
            private const string SpecifiedProductCannotFind = "Specified product is not found.";
            private const string InsufficientStock = "Insufficient stock to complete the sale.";

            private readonly ILogger _logger;
            private readonly IUnitOfWork _unitOfWork;
            public AddSalesRequestHandler(ILogger logger, IUnitOfWork unitOfWork)
            {
                _logger = logger;
                _unitOfWork = unitOfWork;
            }

            public async Task<DataResult<List<AddSalesResponse>>> Handle(AddSalesRequest request, CancellationToken cancellationToken)
            {
                var validator = new AddSalesRequestValidator();
                var validationResult = validator.Validate(request);

                if (!validationResult.IsValid)
                {
                    return DataResult<List<AddSalesResponse>>.Invalid(validationResult.Errors.First().ErrorMessage);
                }

                try
                {
                    if (await _unitOfWork.Products.CountAsync(p => p.Id == request.ProductId) == 0)
                    {
                        return DataResult<List<AddSalesResponse>>.Invalid(SpecifiedProductCannotFind);
                    }

                    // .ContinueWith() can be used
                    var productSupplies = await _unitOfWork.ProductSupplies
                        .FindAsync(ps => ps.ProductId == request.ProductId && ps.RemainingQuantity > 0 && ps.Date < request.Date);
                    var orderedProductSupplies = productSupplies.OrderBy(ps => ps.Date).ToList();


                    int totalAvailableStock = orderedProductSupplies.Sum(ps => ps.RemainingQuantity);
                    if (totalAvailableStock < request.Quantity)
                        return DataResult<List<AddSalesResponse>>.Invalid(InsufficientStock);


                    int saleQuantity = request.Quantity;
                    foreach (var orderedProductSupply in orderedProductSupplies)
                    {
                        if (saleQuantity == 0)
                            break;

                        if (orderedProductSupply.RemainingQuantity >= saleQuantity)
                        {
                            orderedProductSupply.RemainingQuantity -= saleQuantity;
                            saleQuantity = 0;
                        }
                        else
                        {
                            saleQuantity -= orderedProductSupply.RemainingQuantity;
                            orderedProductSupply.RemainingQuantity = 0;
                        }
                        await _unitOfWork.ProductSupplies.Update(orderedProductSupply);
                    }

                    var productSale = new ProductSale
                    {
                        ProductId = request.ProductId,
                        Quantity = request.Quantity,
                        Date = request.Date
                    };
                    
                    await _unitOfWork.ProductSales.AddAsync(productSale);
                    await _unitOfWork.CommitAsync();

                    var result = orderedProductSupplies.Select(ops => new AddSalesResponse
                    {
                        Id = ops.Id,
                        ProductId = ops.ProductId,
                        Quantity = ops.Quantity,
                        Date = ops.Date,
                        RemainingQuantity = ops.RemainingQuantity
                    }).ToList();

                    return DataResult<List<AddSalesResponse>>.Success(result);
                }
                catch (Exception ex)
                {
                    _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);

                    return DataResult<List<AddSalesResponse>>.Error(ex.Message);
                }
            }
        }
    }
}
