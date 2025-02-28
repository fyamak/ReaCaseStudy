using Business.EventHandlers.Auth;
using Business.Mediator.Behaviours.Requests;
using Business.Services.Security.Auth.Jwt;
using FluentValidation;
using Infrastructure.Data.Postgres;
using Infrastructure.Data.Postgres.Entities;
using MediatR;
using Serilog;
using Serilog.Events;
using Shared.Extensions;
using Shared.Models.Results;

namespace Business.RequestHandlers.Auth;

public abstract class ResetPassword
{
    public class ResetPasswordRequest : IRequest<Result>, IRequestToValidate
    {
        public string Email { get; set; } = default!;
    }

    public class ResetPasswordValidator : AbstractValidator<ResetPasswordRequest>
    {
        public ResetPasswordValidator()
        {
            RuleFor(x => x.Email).EmailAddress();
        }
    }

    public class ResetPasswordRequestHandler : IRequestHandler<ResetPasswordRequest, Result>
    {
        private static string UserNotFound(string email) => $"User with email {email} does not exist";

        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger     _logger;

        public ResetPasswordRequestHandler(IUnitOfWork unitOfWork, ILogger logger)
        {
            _unitOfWork = unitOfWork;
            _logger     = logger.ForContext("SourceContext", GetType().FullName);
        }

        public async Task<Result> Handle(ResetPasswordRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user == null)
                {
                    return Result.Invalid(UserNotFound(request.Email));
                }

                var token = new UserToken(TokenType.ResetPassword,
                    DateTime.UtcNow.AddDays(TokenConstants.ResetPasswordTokenValidUntilDays), user!.Id);

                var userTokensToRemove = await _unitOfWork.UserTokens.FindAsync(userToken =>
                    userToken.UserId    == user.Id && userToken.Token != token.Token &&
                    userToken.TokenType == TokenType.ResetPassword);

                _unitOfWork.UserTokens.RemoveRange(userTokensToRemove);

                token.AddEvent(new ResetPasswordTokenCreated.ResetPasswordTokenCreatedEvent { Token = token.Token, UserEmail = user.Email, UserId = user.Id });

                await _unitOfWork.UserTokens.AddAsync(token);

                await _unitOfWork.CommitAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);

                return Result.Error(ex.Message);
            }
        }
    }
}
