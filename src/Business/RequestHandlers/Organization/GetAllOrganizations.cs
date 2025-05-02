using Infrastructure.Data.Postgres;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Shared.Extensions;
using Shared.Models.Results;

namespace Business.RequestHandlers.Organization;

public abstract class GetAllOrganizations
{
    public class GetAllOrganizationsRequest : IRequest<DataResult<List<GetAllOrganizationsResponse>>>
    {
    }

    public class GetAllOrganizationsResponse
    {
        public int Id { get; set; }
        public string Name{ get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }

    public class GetAllOrganizationsRequestHandler : IRequestHandler<GetAllOrganizationsRequest, DataResult<List<GetAllOrganizationsResponse>>>
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;

        public GetAllOrganizationsRequestHandler(ILogger logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<DataResult<List<GetAllOrganizationsResponse>>> Handle(GetAllOrganizationsRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var organizations = await _unitOfWork.Organizations.GetAllAsync();

                var result = organizations.Select(p => new GetAllOrganizationsResponse
                {
                    Id = p.Id,
                    Name = p.Name,
                    Email = p.Email,
                    Phone = p.Phone,
                    Address = p.Address,
                }).ToList();

                return DataResult<List<GetAllOrganizationsResponse>>.Success(result);
            }
            catch(Exception ex)
            {
                _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);
                return DataResult<List<GetAllOrganizationsResponse>>.Error(ex.Message);
            }
        }
    }
}
