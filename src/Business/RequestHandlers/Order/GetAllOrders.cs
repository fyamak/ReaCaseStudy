using Infrastructure.Data.Postgres;
using MediatR;
using Serilog;
using Serilog.Events;
using Shared.Extensions;
using Shared.Models.Results;
using static Business.RequestHandlers.Product.GetAllProducts;

namespace Business.RequestHandlers.Order;

public abstract class GetAllOrders
{
    public class GetAllOrdersRequest : IRequest<DataResult<List<GetAllOrdersResponse>>>
    {
    }
    public class GetAllOrdersResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int OrganizationId { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
    }

    public class OrdersRequestHandler : IRequestHandler<GetAllOrdersRequest, DataResult<List<GetAllOrdersResponse>>>
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;

        public OrdersRequestHandler(ILogger logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<DataResult<List<GetAllOrdersResponse>>> Handle(GetAllOrdersRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var orders = await _unitOfWork.Orders.FindAsync(ps => !ps.IsDeleted);
                var orderedOrders= orders.OrderBy(ps => ps.Date).ToList();


                if (!orderedOrders.Any())
                {
                    return DataResult<List<GetAllOrdersResponse>>.Invalid("No orders found.");
                }

                var result = orderedOrders.Select(p => new GetAllOrdersResponse
                {
                    Id = p.Id,
                    ProductId = p.ProductId,
                    OrganizationId = p.OrganizationId,
                    Quantity = p.Quantity,
                    Price = p.Price,
                    Date = p.Date,
                    Type = p.Type
                }).ToList();

                return DataResult<List<GetAllOrdersResponse>>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);

                return DataResult<List<GetAllOrdersResponse>>.Error(ex.Message);
            }
        }
    }
}
