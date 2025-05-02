using Infrastructure.Data.Postgres;
using MediatR;
using Serilog;
using Serilog.Events;
using Shared.Extensions;
using Shared.Models.Results;

namespace Business.RequestHandlers.Category;

public abstract class DeleteCategory
{
    public class DeleteCategoryRequest : IRequest<DataResult<string>>
    {
        public int Id { get; set; }
    }

    public class DeleteCategoryRequestHandler : IRequestHandler<DeleteCategoryRequest, DataResult<string>>
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCategoryRequestHandler(ILogger logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<DataResult<string>> Handle(DeleteCategoryRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var category = await _unitOfWork.Categories.FirstOrDefaultAsync(c => c.Id == request.Id);
                if (category == null)
                {
                    return DataResult<string>.Invalid("Invalid category Id");
                }

                await _unitOfWork.Categories.SoftDelete(category);
                await _unitOfWork.CommitAsync();
                return DataResult<string>.Success($"Category with id {category.Id} is deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);
                return DataResult<string>.Error(ex.Message);
            }
        }
    }

}
