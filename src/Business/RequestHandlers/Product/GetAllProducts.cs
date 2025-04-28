using MediatR;
using Serilog.Events;
using Shared.Models.Results;
using Serilog;
using Shared.Extensions;
using Infrastructure.Data.Postgres;


namespace Business.RequestHandlers.Product
{
    public abstract class GetAllProducts
    {
        public class GetAllProductsRequest : IRequest<DataResult<List<GetAllProductsResponse>>> 
        {
        }

        public class GetAllProductsResponse
        {
            public int Id { get; set; }
            public string SKU { get; set; }
            public string Name { get; set; }
            public int TotalQuantity { get; set; }
            public double Price { get; set; }
            public string Category { get; set; }
        }

        public class GetAllProductsRequestHandler : IRequestHandler<GetAllProductsRequest, DataResult<List<GetAllProductsResponse>>>
        {
            private readonly ILogger _logger;
            private readonly IUnitOfWork _unitOfWork;

            public GetAllProductsRequestHandler(ILogger logger, IUnitOfWork unitOfWork)
            {
                _logger = logger;
                _unitOfWork = unitOfWork;
            }

            public async Task<DataResult<List<GetAllProductsResponse>>> Handle(GetAllProductsRequest request, CancellationToken cancellationToken)
            {
                try
                {
                    var products = await _unitOfWork.Products.GetAllAsync();

                    if (products == null || !products.Any())
                    {
                        return DataResult<List<GetAllProductsResponse>>.Invalid("No products found.");
                    }

                    var result = products.Select(p => new GetAllProductsResponse
                    {
                        Id = p.Id,
                        SKU = p.SKU,
                        Name = p.Name,
                        TotalQuantity = p.TotalQuantity,
                        Category = p.Category
                    }).ToList();

                    return DataResult<List<GetAllProductsResponse>>.Success(result);
                }
                catch (Exception ex)
                {
                    _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);

                    return DataResult<List<GetAllProductsResponse>>.Error(ex.Message);
                }
            }
        }
    }
}
