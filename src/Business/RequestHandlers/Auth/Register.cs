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

public abstract class Register
{
    public class RegisterRequest : IRequest<DataResult<Token>>, IRequestToValidate
    {
        public string Email    { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string FullName { get; set; } = default!;
    }

    public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
    {
        public RegisterRequestValidator()
        {
            RuleFor(x => x.Email).EmailAddress();
            RuleFor(x => x.Password).MinimumLength(8);
            RuleFor(x => x.FullName).NotEmpty().MinimumLength(6)
                .Must(x => string.IsNullOrEmpty(x) || x.All(c => char.IsLetter(c) || char.IsWhiteSpace(c)))
                .WithMessage("Full name must contain only letters");
        }
    }

    public class RegisterRequestHandler : IRequestHandler<RegisterRequest, DataResult<Token>>
    {
        private readonly IUnitOfWork                 _unitOfWork;
        private readonly IJwtTokenService            _jwtTokenService;
        private readonly IUserPasswordHashingService _userPasswordHashingService;
        private readonly ILogger                     _logger;

        private const string UserWithSameEmailAlreadyExists = "User with same email already exists";

        public RegisterRequestHandler(
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

        public async Task<DataResult<Token>> Handle(RegisterRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (await _unitOfWork.Users.CountAsync(u => u.Email == request.Email) > 0)
                {
                    return DataResult<Token>.Invalid(UserWithSameEmailAlreadyExists);
                }

                _userPasswordHashingService.CreatePasswordHash(request.Password, out var passwordHash, out var passwordSalt);

                var user = new Infrastructure.Data.Postgres.Entities.User
                {
                    Email        = request.Email,
                    FullName     = request.FullName,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    UserType     = UserType.User
                };

                await _unitOfWork.Users.AddAsync(user);

                await _unitOfWork.CommitAsync();

                var refreshToken = new UserToken(TokenType.RefreshToken, DateTime.UtcNow.AddDays(TokenConstants.RefreshTokenValidUntilDays), user.Id);

                var token = _jwtTokenService.CreateAccessToken(user, refreshToken.Token);

                await _unitOfWork.UserTokens.AddAsync(refreshToken);

                await _unitOfWork.CommitAsync();

                return DataResult<Token>.Success(token);
            }
            catch (Exception ex)
            {
                _logger.LogExtended(LogEventLevel.Error, $"Error on {GetType().Name}", ex);

                return DataResult<Token>.Error(ex.Message);
            }
        }
    }
}
