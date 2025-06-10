using Business.Mediator.Behaviours.Requests;
using FluentValidation;
using Infrastructure.Data.Postgres;
using MediatR;
using Serilog;
using Serilog.Events;
using Shared.Extensions;
using Shared.Models.Results;

namespace Business.RequestHandlers.Product;

public abstract class GetPagedProducts
{
    public class GetPagedProductsRequest : IRequest<PagedResult<GetPagedProductsResponse>>, IRequestToValidate
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
    }

    public class GetPagedProductsResponse
    {
        public int Id { get; set; }
        public string SKU { get; set; }
        public string Name { get; set; }
        public int TotalQuantity { get; set; }
        public double Price { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    }

    public class GetPagedProductsRequestValidator : AbstractValidator<GetPagedProductsRequest>
    {
        public GetPagedProductsRequestValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThan(0).WithMessage("Page number must be bigger than 0");
            RuleFor(x => x.PageSize).GreaterThan(0).WithMessage("Page number must be bigger than 0");
        }
    };

    public class GetPagedProductsRequestHandler : IRequestHandler<GetPagedProductsRequest, PagedResult<GetPagedProductsResponse>>
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;

        public GetPagedProductsRequestHandler(ILogger logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<GetPagedProductsResponse>> Handle(GetPagedProductsRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var (products, totalCount) = await _unitOfWork.Products.GetPagedProductsAsync(
                    pageNumber: request.PageNumber,
                    pageSize: request.PageSize,
                    search: request.Search,
                    categoryId: request.CategoryId);

                var result = products.Select(p => new GetPagedProductsResponse
                {
                    Id = p.Id,
                    SKU = p.SKU,
                    Name = p.Name,
                    TotalQuantity = p.TotalQuantity,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name
                }).ToList();

                return PagedResult<GetPagedProductsResponse>.Success(
                    result,
                    request.PageNumber,
                    request.PageSize,
                    totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);

                return PagedResult<GetPagedProductsResponse>.Error(ex.Message);
            }
        }
    }
}
