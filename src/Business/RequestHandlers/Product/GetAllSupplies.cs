using Infrastructure.Data.Postgres;
using MediatR;
using Serilog;
using Serilog.Events;
using Shared.Extensions;
using Shared.Models.Results;

namespace Business.RequestHandlers.Product;

public abstract class GetAllSupplies
{
    public class GetAllSuppliesRequest : IRequest<DataResult<List<GetAllSuppliesResponse>>>
    {
    }

    public class GetAllSuppliesResponse
    {
        public int Id { get; set; }
        public int ProductID { get; set; }
        public int Quantity { get; set; }
        public DateTime Date { get; set; }
        public int RemainingQuantity { get; set; }
    }

    public class GetAllSuppliesRequestHandler : IRequestHandler<GetAllSuppliesRequest, DataResult<List<GetAllSuppliesResponse>>>
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        public GetAllSuppliesRequestHandler(ILogger logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        public async Task<DataResult<List<GetAllSuppliesResponse>>> Handle(GetAllSuppliesRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var supplies = await _unitOfWork.ProductSupplies.GetAllAsync();
                if (supplies == null || !supplies.Any())
                {
                    return DataResult<List<GetAllSuppliesResponse>>.Invalid("No suplly found.");
                }

                var result = supplies.Select(p => new GetAllSuppliesResponse
                {
                    Id = p.Id,
                    ProductID = p.ProductId,
                    Quantity = p.Quantity,
                    Date = p.Date,
                    RemainingQuantity = p.RemainingQuantity
                }).ToList();

                return DataResult<List<GetAllSuppliesResponse>>.Success(result);

            }
            catch(Exception ex)
            { 
                _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);
                return DataResult<List<GetAllSuppliesResponse>>.Error(ex.Message);
            }

        }
    }

}
