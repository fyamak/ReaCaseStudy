using Infrastructure.Data.Postgres;
using MediatR;
using Serilog;
using Serilog.Events;
using Shared.Extensions;
using Shared.Models.Results;


namespace Business.RequestHandlers.Category;

public abstract class GetAllCategories
{
    public class GetAllCategoriesRequest : IRequest<DataResult<List<GetAllCategoriesResponse>>>
    {
    }

    public class GetAllCategoriesResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class GetAllCategoriesRequestHandler : IRequestHandler<GetAllCategoriesRequest, DataResult<List<GetAllCategoriesResponse>>>
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;

        public GetAllCategoriesRequestHandler(ILogger logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<DataResult<List<GetAllCategoriesResponse>>> Handle(GetAllCategoriesRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var categories = await _unitOfWork.Categories.GetAllAsync();
                
                var result = categories.Select(c => new GetAllCategoriesResponse
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToList();

                return DataResult<List<GetAllCategoriesResponse>>.Success(result);
            }
            catch(Exception ex)
            {
                _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);
                return DataResult<List<GetAllCategoriesResponse>>.Error(ex.Message);
            }
            throw new NotImplementedException();
        }
    }

}
