using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure.Data.Postgres;
using MediatR;
using Shared.Models.Results;
using Business.Services.Security.Auth.Jwt.Interface;
using Serilog;

namespace Business.RequestHandlers.User;

public abstract class EditProfileInfo
{
    public class EditProfileInfoRequest : IRequest<DataResult<string>>
    {
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Currency { get; set; }
        public bool? ReceiveEmail { get; set; }
        public bool? ReceiveLowStockAlert { get; set; }
    }

    public class EditProfileInfoRequestHandler : IRequestHandler<EditProfileInfoRequest, DataResult<string>>
    {
        private const string UserNotFound = "User not found";
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserContext _userContext;
        private readonly ILogger _logger;
        public EditProfileInfoRequestHandler(IUnitOfWork unitOfWork, IUserContext userContext, ILogger logger)
        {
            _unitOfWork = unitOfWork;
            _userContext = userContext;
            _logger = logger.ForContext("SourceContext", GetType().FullName);
        }
        public async Task<DataResult<string>> Handle(EditProfileInfoRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var userId = _userContext.GetUserId();
                var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
                if (user == null)
                {
                    return DataResult<string>.Invalid(UserNotFound);
                }

                if (!string.IsNullOrWhiteSpace(request.FullName))
                    user.FullName = request.FullName;

                if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                    user.PhoneNumber = request.PhoneNumber;

                if (!string.IsNullOrWhiteSpace(request.Currency))
                    user.Currency = request.Currency;

                if (request.ReceiveEmail.HasValue)
                    user.ReceiveEmail = request.ReceiveEmail.Value;

                if (request.ReceiveLowStockAlert.HasValue)
                    user.ReceiveLowStockAlert = request.ReceiveLowStockAlert.Value;

                user.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.Users.Update(user);
                await _unitOfWork.CommitAsync();

                return DataResult<string>.Success("Profile updated successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error while updating profile");
                return DataResult<string>.Error("An error occurred while updating the profile");
            }
        }
    }
}
