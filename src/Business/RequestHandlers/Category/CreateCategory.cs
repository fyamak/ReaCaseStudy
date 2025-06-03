

using Infrastructure.Data.Postgres;
using Infrastructure.Data.Postgres.Entities;
using MediatR;
using Serilog;
using Serilog.Events;
using Shared.Extensions;
using Shared.Models.Results;

namespace Business.RequestHandlers.Category;

public abstract class CreateCategory
{
    public class CreateCategoryRequest : IRequest<DataResult<CreateCategoryResponse>>
    {
        public string Name { get; set; }
    }

    public class CreateCategoryResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class CreateCategoryRequestHandler : IRequestHandler<CreateCategoryRequest, DataResult<CreateCategoryResponse>>
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;

        public CreateCategoryRequestHandler(ILogger logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<DataResult<CreateCategoryResponse>> Handle(CreateCategoryRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var categoryCheck = await _unitOfWork.Categories.FirstOrDefaultAsync(x => x.Name == request.Name);
                if(categoryCheck != null)
                {
                    return DataResult<CreateCategoryResponse>.Invalid("Category with same name is already exist");
                }

                var category = new Infrastructure.Data.Postgres.Entities.Category
                {
                    Name = request.Name
                };

                await _unitOfWork.Categories.AddAsync(category);
                await _unitOfWork.CommitAsync();

                var result = new CreateCategoryResponse
                {
                    Id = category.Id,
                    Name = category.Name
                };

                return DataResult<CreateCategoryResponse>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);
                return DataResult<CreateCategoryResponse>.Error(ex.Message);
            }
        }
    }
}
