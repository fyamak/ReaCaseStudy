using System.Text.Json.Serialization;
using Business.Services.Security.Auth.Jwt.Interface;
using Infrastructure.Data.Postgres;
using Infrastructure.Data.Postgres.Entities;
using MediatR;
using Serilog;
using Serilog.Events;
using Shared.Extensions;
using Shared.Models.Results;

namespace Business.RequestHandlers.User;

public abstract class GetProfileInfo
{
    public class GetProfileInfoRequest : IRequest<DataResult<GetProfileInfoResponse>>
    {
    }

    public class GetProfileInfoResponse
    {
        public int    Id       { get; set; }
        public string FullName { get; set; } = default!;
        public string Email    { get; set; } = default!;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserType UserType { get; set; }
    }

    public class GetProfileInfoRequestHandler : IRequestHandler<GetProfileInfoRequest,
        DataResult<GetProfileInfoResponse>>
    {
        private const string UserNotFound = "User not found";

        private readonly IUnitOfWork  _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly ILogger      _logger;

        public GetProfileInfoRequestHandler(IUnitOfWork unitOfWork, IUserContext userContext, ILogger logger)
        {
            _unitOfWork  = unitOfWork;
            _userContext = userContext;
            _logger      = logger.ForContext("SourceContext", GetType().FullName);
        }

        public async Task<DataResult<GetProfileInfoResponse>> Handle(GetProfileInfoRequest request,
            CancellationToken                                                              cancellationToken)
        {
            try
            {
                var userId = _userContext.GetUserId();

                var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

                if (user == null)
                {
                    return DataResult<GetProfileInfoResponse>.Invalid(UserNotFound);
                }

                var result = new GetProfileInfoResponse { Id = user.Id, Email = user.Email, FullName = user.FullName, UserType = user.UserType };

                return DataResult<GetProfileInfoResponse>.Success(result);
            }
            catch (Exception ex)
            {
                _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);

                return DataResult<GetProfileInfoResponse>.Error(ex.Message);
            }
        }
    }
}
