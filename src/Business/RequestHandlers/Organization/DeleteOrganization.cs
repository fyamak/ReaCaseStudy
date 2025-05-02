using Infrastructure.Data.Postgres;
using MediatR;
using Serilog;
using Serilog.Events;
using Shared.Extensions;
using Shared.Models.Results;

namespace Business.RequestHandlers.Organization;

public abstract class DeleteOrganization
{
    public class DeleteOrganizationRequest : IRequest<DataResult<string>>
    {
        public int Id { get; set; }
    }

    public class DeleteOrganizationRequestHandler : IRequestHandler<DeleteOrganizationRequest, DataResult<string>>
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteOrganizationRequestHandler(ILogger logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public async Task<DataResult<string>> Handle(DeleteOrganizationRequest request, CancellationToken cancellationToken)
        {

            try
            {
                var organization = await _unitOfWork.Organizations.FirstOrDefaultAsync(p => p.Id == request.Id);
                if(organization == null)
                {
                    return DataResult<string>.Invalid("Invalid organization Id");
                }

                await _unitOfWork.Organizations.SoftDelete(organization);
                await _unitOfWork.CommitAsync();
                return DataResult<string>.Success($"Organization with id {organization.Id} is deleted successfully");   
            }
            catch (Exception ex)
            {
                _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);

                return DataResult<string>.Error(ex.Message);
            }

        }
    }
}
