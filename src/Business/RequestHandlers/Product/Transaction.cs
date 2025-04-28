using MediatR;
using Serilog.Events;
using Shared.Models.Results;
using Serilog;
using Shared.Extensions;
using Infrastructure.Data.Postgres;
using FluentValidation;


namespace Business.RequestHandlers.Product
{
    public class Transaction
    {
        public class TransactionRequest : IRequest<DataResult<List<TransactionResponse>>>
        {
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public int? ProductId { get; set; }
        }

        public class TransactionResponse
        {
            public int Id { get; set; }
            public string Product { get; set; }
            public string Organization { get; set; }
            public string Type { get; set; }
            public double Price { get; set; }
            public int Quantity { get; set; }
            public DateTime Date { get; set; }
            public int? RemainingQuantity { get; set; }
        }

        public class TransactionRequestValidator: AbstractValidator<TransactionRequest>
        {
            public TransactionRequestValidator()
            {
                RuleFor(x => x.StartDate).NotEmpty();
                RuleFor(x => x.EndDate).NotEmpty();
                RuleFor(x => x)
                    .Must(x => x.EndDate > x.StartDate)
                    .WithMessage("End date cannot be earlier than start date.");
            }
        }

        public class TransactionRequestHandler : IRequestHandler<TransactionRequest, DataResult<List<TransactionResponse>>>
        {
            private readonly ILogger _logger;
            private readonly IUnitOfWork _unitOfWork;

            public TransactionRequestHandler(ILogger logger, IUnitOfWork unitOfWork)
            {
                _logger = logger;
                _unitOfWork = unitOfWork;
            }

            public async Task<DataResult<List<TransactionResponse>>> Handle(TransactionRequest request, CancellationToken cancellationToken)
            {
                var validator = new TransactionRequestValidator();
                var validationResult = validator.Validate(request);

                if (!validationResult.IsValid)
                {
                    return DataResult<List<TransactionResponse>>.Invalid(validationResult.Errors.First().ErrorMessage);
                }

                try
                {
                    var productSupplies = await _unitOfWork.ProductSupplies
                        .FindAsync(p => p.Date >= request.StartDate
                            && p.Date <= request.EndDate
                            && (!request.ProductId.HasValue || p.ProductId == request.ProductId));
                    var orderedProductSupplies = productSupplies.OrderBy(ps => ps.Date).ToList();

                    var productSales = await _unitOfWork.ProductSales
                        .FindAsync(s => s.Date >= request.StartDate
                            && s.Date <= request.EndDate
                            && (!request.ProductId.HasValue || s.ProductId == request.ProductId));
                    var orderedProductSales = productSales.OrderBy(ps => ps.Date).ToList();


                    var productIds = orderedProductSupplies.Select(x => x.ProductId)
                        .Concat(orderedProductSales.Select(x => x.ProductId))
                        .Distinct().ToList();

                    var organizationIds = orderedProductSupplies.Select(x => x.OrganizationId)
                        .Concat(orderedProductSales.Select(x => x.OrganizationId))
                        .Distinct().ToList();

                    var products = await _unitOfWork.Products.FindAsync(p => productIds.Contains(p.Id));
                    var organizations = await _unitOfWork.Organizations.FindAsync(o => organizationIds.Contains(o.Id));

                    var productNameDict = products.ToDictionary(p => p.Id, p => p.Name);
                    var organizationNameDict = organizations.ToDictionary(o => o.Id, o => o.Name);



                    var transactions = orderedProductSupplies.Select(p => new TransactionResponse
                    {
                        Id = p.Id,
                        Product = productNameDict.GetValueOrDefault(p.ProductId, "Unknown Product"),
                        Organization = organizationNameDict.GetValueOrDefault(p.OrganizationId, "Unknown Organization"),
                        Type = "Supply",
                        Price = p.Price,
                        Quantity = p.Quantity,
                        Date = p.Date,
                        RemainingQuantity = p.RemainingQuantity
                    }).ToList();
                    
                    transactions.AddRange(orderedProductSales.Select(s => new TransactionResponse
                    {
                        Id = s.Id,
                        Product = productNameDict.GetValueOrDefault(s.ProductId, "Unknown Product"),
                        Organization = organizationNameDict.GetValueOrDefault(s.OrganizationId, "Unknown Organization"),
                        Type = "Sale",
                        Price = s.Price,
                        Quantity = s.Quantity,
                        Date = s.Date
                    }));

                    transactions = transactions.OrderBy(t => t.Date).ToList();

                    return DataResult<List<TransactionResponse>>.Success(transactions);
                }
                catch (Exception ex)
                {
                    _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);

                    return DataResult<List<TransactionResponse>>.Error(ex.Message);
                }
            }
        }
    }
}
