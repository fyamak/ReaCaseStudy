using Business.Mediator.Behaviours.Requests;
using FluentValidation;
using Infrastructure.Data.Postgres;
using MediatR;
using Serilog;
using Serilog.Events;
using Shared.Extensions;
using Shared.Models.Results;

namespace Business.RequestHandlers.Category;

public abstract class GetPagedCategories
{
    public class GetPagedCategoriesRequest : IRequest<PagedResult<GetPagedCategoriesResponse>>, IRequestToValidate
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? Search { get; set; }
    }

    public class GetPagedCategoriesResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class GetPagedCategoriesRequestValidator : AbstractValidator<GetPagedCategoriesRequest>
    {
        public GetPagedCategoriesRequestValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThan(0).WithMessage("Page number must be bigger than 0");
            RuleFor(x => x.PageSize).GreaterThan(0).WithMessage("Page number must be bigger than 0");
        }
    }

    public class GetPagedCategoriesRequestHandler : IRequestHandler<GetPagedCategoriesRequest, PagedResult<GetPagedCategoriesResponse>>
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;

        public GetPagedCategoriesRequestHandler(ILogger logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<GetPagedCategoriesResponse>> Handle(GetPagedCategoriesRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var (categories, totalCount) = await _unitOfWork.Categories.GetPagedCategoriesAsync(
                    pageNumber: request.PageNumber,
                    pageSize: request.PageSize,
                    search: request.Search);

                var result = categories.Select(c => new GetPagedCategoriesResponse
                {
                    Id = c.Id,
                    Name = c.Name
                }).ToList();

                return PagedResult<GetPagedCategoriesResponse>.Success(
                    result,
                    request.PageNumber,
                    request.PageSize,
                    totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);
                return PagedResult<GetPagedCategoriesResponse>.Error(ex.Message);
            }
        }
    }
}
