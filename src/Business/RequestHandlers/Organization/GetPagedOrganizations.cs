using System.Linq.Expressions;
using Business.Mediator.Behaviours.Requests;
using FluentValidation;
using Infrastructure.Data.Postgres;
using Infrastructure.Data.Postgres.Entities;
using MediatR;
using Serilog;
using Serilog.Events;
using Shared.Extensions;
using Shared.Models.Results;

namespace Business.RequestHandlers.Organization;

public abstract class GetPagedOrganizations
{
    public class GetPagedOrganizationsRequest : IRequest<PagedResult<GetPagedOrganizationsResponse>>, IRequestToValidate
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? Search { get; set; }
    }

    public class GetPagedOrganizationsResponse
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }

    public class GetPagedOrganizationsRequestValidator : AbstractValidator<GetPagedOrganizationsRequest>
    {
        public GetPagedOrganizationsRequestValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThan(0).WithMessage("Page number must be bigger than 0");
            RuleFor(x => x.PageSize).GreaterThan(0).WithMessage("Page number must be bigger than 0");
        }
    }

    public class GetPagedOrganizationsRequestHandler : IRequestHandler<GetPagedOrganizationsRequest, PagedResult<GetPagedOrganizationsResponse>>
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;

        public GetPagedOrganizationsRequestHandler(ILogger logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;

            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<GetPagedOrganizationsResponse>> Handle(GetPagedOrganizationsRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var (organizations, totalCount) = await _unitOfWork.Organizations.GetPagedOrganizationsAsync(
                    pageNumber: request.PageNumber,
                    pageSize: request.PageSize,
                    search: request.Search);

                var result = organizations.Select(p => new GetPagedOrganizationsResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Email = p.Email,
                    Phone = p.Phone,
                    Address = p.Address,
                }).ToList();

                return PagedResult<GetPagedOrganizationsResponse>.Success(
                    result,
                    request.PageNumber,
                    request.PageSize,
                    totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);
                return PagedResult<GetPagedOrganizationsResponse>.Error(ex.Message);
            }
        }


    }
}
