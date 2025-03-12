using Infrastructure.Data.Postgres;
using MediatR;
using Shared.Models.Results;
using Serilog;
using Serilog.Events;
using Shared.Extensions;
using Infrastructure.Data.Postgres.Entities;
using FluentValidation;


namespace Business.RequestHandlers.Product
{
    public class AddSupply
    {
        public class AddSupplyRequest : IRequest<DataResult<AddSupplyResponse>>
        {
            public int ProductId { get; internal set; }
            public int Quantity { get; set; }
            public DateTime Date { get; set; }
        }

        public class AddSupplyResponse
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public int Quantity { get; set; }
            public DateTime Date { get; set; }
            public int RemainingQuantity { get; set; }
        }

        public class AddSupplyRequestValidator : AbstractValidator<AddSupplyRequest>
        {
            public AddSupplyRequestValidator()
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

        public class AddSupplyRequestHandler : IRequestHandler<AddSupplyRequest, DataResult<AddSupplyResponse>>
        {
            private const string SpecifiedProductCannotFind = "Specified product is not found.";

            private readonly IUnitOfWork _unitOfWork;
            private readonly ILogger _logger;
            public AddSupplyRequestHandler(IUnitOfWork unitOfWork, ILogger logger)
            {
                _unitOfWork = unitOfWork;
                _logger = logger;
            }

            public async Task<DataResult<AddSupplyResponse>> Handle(AddSupplyRequest request, CancellationToken cancellationToken)
            {
                var validator = new AddSupplyRequestValidator();
                var validationResult = validator.Validate(request);

                if (!validationResult.IsValid)
                {
                    return DataResult<AddSupplyResponse>.Invalid(validationResult.Errors.First().ErrorMessage);
                }

                try
                {
                    if (await _unitOfWork.Products.CountAsync(p => p.Id == request.ProductId) == 0)
                    {
                        return DataResult<AddSupplyResponse>.Invalid(SpecifiedProductCannotFind);
                    }

                    var productSupply = new ProductSupply
                    {
                        ProductId = request.ProductId,
                        Quantity = request.Quantity,
                        Date = request.Date,
                        RemainingQuantity = request.Quantity
                    };
                    await _unitOfWork.ProductSupplies.AddAsync(productSupply);
                    await _unitOfWork.CommitAsync();

                    return DataResult<AddSupplyResponse>.Success(new AddSupplyResponse
                    {
                        Id = productSupply.Id,
                        ProductId = productSupply.ProductId,
                        Quantity = productSupply.Quantity,
                        Date = productSupply.Date,
                        RemainingQuantity = productSupply.RemainingQuantity
                    });

                }
                catch (Exception ex)
                {
                    _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);

                    return DataResult<AddSupplyResponse>.Error(ex.Message);
                }
            }
        }
    }
}
