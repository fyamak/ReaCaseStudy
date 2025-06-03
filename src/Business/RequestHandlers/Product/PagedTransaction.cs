using MediatR;
using Shared.Models.Results;
using FluentValidation;
using Infrastructure.Data.Postgres;
using Serilog;
using Serilog.Events;
using Shared.Extensions;

namespace Business.RequestHandlers.Product;

public abstract class PagedTransaction
{
    public class PagedTransactionRequest : IRequest<PagedResult<PagedTransactionResponse>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IncludeFailures { get; set; }
        public int? ProductId { get; set; }
    }

    public class PagedTransactionResponse
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string OrganizationName { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string Detail { get; set; }
        public bool? IsSuccessfull { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
    public class TransactionRequestValidator : AbstractValidator<PagedTransactionRequest>
    {
        public TransactionRequestValidator()
        {
            RuleFor(x => x.StartDate).NotEmpty();
            RuleFor(x => x.EndDate).NotEmpty();
            RuleFor(x => x)
                .Must(x => x.EndDate > x.StartDate)
                .WithMessage("End date cannot be earlier than start date.");
        }
    }

    public class PagedTransactionRequestHandler : IRequestHandler<PagedTransactionRequest, PagedResult<PagedTransactionResponse>>
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;

        public PagedTransactionRequestHandler(ILogger logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<PagedTransactionResponse>> Handle(PagedTransactionRequest request, CancellationToken cancellationToken)
        {
            var validator = new TransactionRequestValidator();
            var validationResult = validator.Validate(request);

            if (!validationResult.IsValid)
            {
                return PagedResult<PagedTransactionResponse>.Invalid(validationResult.Errors.First().ErrorMessage);
            }

            try
            {
                var (transactions, totalCount) = await _unitOfWork.Products.GetPagedTransactionAsync(
                    pageNumber: request.PageNumber,
                    pageSize: request.PageSize,
                    startDate: request.StartDate,
                    endDate: request.EndDate,
                    includeFailures: request.IncludeFailures,
                    productId: request.ProductId);

                var result = transactions.Select(p => new PagedTransactionResponse
                {
                    Id = p.Id,
                    ProductName = p.Product.Name,
                    OrganizationName = p.Organization.Name,
                    Quantity = p.Quantity,
                    Price = p.Price,
                    Date = p.Date,
                    Type = p.Type,
                    Detail = p.Detail,
                    IsSuccessfull = p.IsSuccessfull,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList();

                return PagedResult<PagedTransactionResponse>.Success(
                    result,
                    request.PageNumber,
                    request.PageSize,
                    totalCount);
            }

            catch (Exception ex)
            {
                _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);

                return PagedResult<PagedTransactionResponse>.Error(ex.Message);
            }

            throw new NotImplementedException();
        }
    }
}
