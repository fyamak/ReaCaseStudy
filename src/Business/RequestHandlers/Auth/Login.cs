using System.ComponentModel;
using Business.Mediator.Behaviours.Requests;
using Business.Services.Security.Auth.Jwt;
using Business.Services.Security.Auth.Jwt.Interface;
using Business.Services.Security.Auth.Jwt.Models;
using Business.Services.Security.Auth.UserPassword.Interface;
using FluentValidation;
using Infrastructure.Data.Postgres;
using Infrastructure.Data.Postgres.Entities;
using MediatR;
using Serilog;
using Serilog.Events;
using Shared.Extensions;
using Shared.Models.Results;

namespace Business.RequestHandlers.Auth;

public abstract class Login
{
    public class LoginRequest : IRequest<DataResult<Token>>, IRequestToValidate
    {
        [DefaultValue("fy@gmail.com")]
        public string Email    { get; set; } = default!;
        [DefaultValue("12345678")]
        public string Password { get; set; } = default!;
    }

    public class LoginRequestValidator : AbstractValidator<LoginRequest>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Please enter a valid email address.");
            RuleFor(x => x.Password).NotEmpty();
        }
    }

    public class LoginRequestHandler : IRequestHandler<LoginRequest, DataResult<Token>>
    {
        private const string EmailOrPasswordIsWrong = "Wrong email or password";

        private readonly IUnitOfWork                 _unitOfWork;
        private readonly IJwtTokenService            _jwtTokenService;
        private readonly IUserPasswordHashingService _userPasswordHashingService;
        private readonly ILogger                     _logger;

        public LoginRequestHandler(
            IUnitOfWork                 unitOfWork,
            IJwtTokenService            jwtTokenService,
            IUserPasswordHashingService userPasswordHashingService,
            ILogger                     logger)
        {
            _unitOfWork                 = unitOfWork;
            _jwtTokenService            = jwtTokenService;
            _userPasswordHashingService = userPasswordHashingService;
            _logger                     = logger.ForContext("SourceContext", GetType().FullName);
        }

        public async Task<DataResult<Token>> Handle(LoginRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted);

                if (user == null || !_userPasswordHashingService.VerifyPasswordHash(request.Password, user.PasswordHash, user.PasswordSalt))
                {
                    return DataResult<Token>.Invalid(EmailOrPasswordIsWrong);
                }

                var refreshToken = new UserToken(TokenType.RefreshToken,
                    DateTime.UtcNow.AddDays(TokenConstants.RefreshTokenValidUntilDays), user.Id);

                var jwtToken = _jwtTokenService.CreateAccessToken(user, refreshToken.Token);

                await _unitOfWork.UserTokens.AddAsync(refreshToken);

                await _unitOfWork.CommitAsync();

                return DataResult<Token>.Success(jwtToken);
            }
            catch (Exception ex)
            {
                _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);

                return DataResult<Token>.Error(ex.Message);
            }
        }
    }
}
