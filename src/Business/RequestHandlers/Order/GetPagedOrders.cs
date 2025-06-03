using Infrastructure.Data.Postgres;
using MediatR;
using Serilog;
using Serilog.Events;
using Shared.Extensions;
using Shared.Models.Results;

namespace Business.RequestHandlers.Order;

public abstract class GetPagedOrders
{
    public class GetPagedOrdersRequest : IRequest<PagedResult<GetPagedOrdersResponse>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public bool IsDeleted { get; set; }
        public string? Search { get; set; }
        public string? Type { get; set; }

    }

    public class GetPagedOrdersResponse
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int OrganizationId { get; set; }
        public string OrganizationName { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public bool? IsSuccessfull { get; set; }
        public string? Detail { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class GetPagedOrdersRequestHandler : IRequestHandler<GetPagedOrdersRequest, PagedResult<GetPagedOrdersResponse>>
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;

        public GetPagedOrdersRequestHandler(ILogger logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<GetPagedOrdersResponse>> Handle(GetPagedOrdersRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var (orders, totalCount) = await _unitOfWork.Orders.GetPagedOrdersAsync(
                    pageNumber: request.PageNumber,
                    pageSize: request.PageSize,
                    isDeleted: request.IsDeleted,
                    search: request.Search,
                    type: request.Type);

                var result = orders.Select(p => new GetPagedOrdersResponse
                {
                    Id = p.Id,
                    ProductId = p.ProductId,
                    ProductName = p.Product.Name,
                    OrganizationId = p.OrganizationId,
                    OrganizationName = p.Organization.Name,
                    Quantity = p.Quantity,
                    Price = p.Price,
                    Date = p.Date,
                    Type = p.Type,
                    IsSuccessfull = p.IsSuccessfull,
                    Detail = p.Detail,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList();

                return PagedResult<GetPagedOrdersResponse>.Success(
                    result,
                    request.PageNumber,
                    request.PageSize,
                    totalCount);
            }

            catch (Exception ex)
            {
                _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);

                return PagedResult<GetPagedOrdersResponse>.Error(ex.Message);
            }
        }
    }
}
