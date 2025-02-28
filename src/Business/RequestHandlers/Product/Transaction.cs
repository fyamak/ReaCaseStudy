using MediatR;
using Serilog.Events;
using Shared.Models.Results;
using Serilog;
using Shared.Extensions;
using Infrastructure.Data.Postgres;


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
            public int ProductId { get; set; }
            public string Type { get; set; } 
            public int Quantity { get; set; }
            public DateTime Date { get; set; }
            public int? RemainingQuantity { get; set; }
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
                try
                {
                    var productSupplies = await _unitOfWork.ProductSupplies.GetAllAsync();
                    var productSales = await _unitOfWork.ProductSales.GetAllAsync();

                    var filteredProductSupplies = productSupplies
                        .Where(p => p.Date >= request.StartDate && p.Date <= request.EndDate &&
                                    (!request.ProductId.HasValue || p.ProductId == request.ProductId))
                        .ToList();

                    var filteredProductSales = productSales
                        .Where(s => s.Date >= request.StartDate && s.Date <= request.EndDate &&
                                    (!request.ProductId.HasValue || s.ProductId == request.ProductId))
                        .ToList();

                    var transactions = filteredProductSupplies.Select(p => new TransactionResponse
                    {
                        Id = p.Id,
                        ProductId = p.ProductId,
                        Type = "Supply",
                        Quantity = p.Quantity,
                        Date = p.Date,
                        RemainingQuantity = p.RemainingQuantity
                    }).ToList();
                    
                    transactions.AddRange(filteredProductSales.Select(s => new TransactionResponse
                    {
                        Id = s.Id,
                        ProductId = s.ProductId,
                        Type = "Sale",
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
