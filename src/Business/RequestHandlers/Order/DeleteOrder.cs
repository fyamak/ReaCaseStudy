using Infrastructure.Data.Postgres;
using MediatR;
using Serilog;
using Serilog.Events;
using Shared.Extensions;
using Shared.Models.Results;

namespace Business.RequestHandlers.Order;

public abstract class DeleteOrder
{
    public class DeleteOrderRequest : IRequest<DataResult<string>>
    {
        public int Id;
    }

    public class DeleteOrderRequestHandler : IRequestHandler<DeleteOrderRequest, DataResult<string>>
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteOrderRequestHandler(ILogger logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<DataResult<string>> Handle(DeleteOrderRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var order = await _unitOfWork.Orders.FirstOrDefaultAsync(p => p.Id == request.Id);

                if (order == null)
                {
                    return DataResult<string>.Invalid("Invalid order Id");
                }

                await _unitOfWork.Orders.SoftDelete(order);
                await _unitOfWork.CommitAsync();

                return DataResult<string>.Success($"Order {request.Id} is successfully deleted.");
            }
            catch (Exception ex)
            {
                _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);

                return DataResult<string>.Error(ex.Message);
            }
        }
    }
}
