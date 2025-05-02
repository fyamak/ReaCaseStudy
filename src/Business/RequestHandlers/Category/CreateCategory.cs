

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
    public class CreateCategoryRequest : IRequest<DataResult<string>>
    {
        public string Name { get; set; }
    }

    public class CreateCategoryRequestHandler : IRequestHandler<CreateCategoryRequest, DataResult<string>>
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;

        public CreateCategoryRequestHandler(ILogger logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<DataResult<string>> Handle(CreateCategoryRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var categoryCheck = await _unitOfWork.Categories.FirstOrDefaultAsync(x => x.Name == request.Name);
                if(categoryCheck != null)
                {
                    return DataResult<string>.Invalid("Category with same name is already exist");
                }

                var category = new Infrastructure.Data.Postgres.Entities.Category
                {
                    Name = request.Name
                };

                await _unitOfWork.Categories.AddAsync(category);
                await _unitOfWork.CommitAsync();

                return DataResult<string>.Success("Category is succesfully created");
            }
            catch (Exception ex)
            {
                _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);
                return DataResult<string>.Error(ex.Message);
            }
        }
    }
}
