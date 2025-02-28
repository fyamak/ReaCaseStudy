using Business.Services.Security.Auth.Jwt;
using Business.Services.Security.Auth.Jwt.Interface;
using Business.Services.Security.Auth.Jwt.Models;
using Infrastructure.Data.Postgres;
using Infrastructure.Data.Postgres.Entities;
using MediatR;
using Serilog;
using Serilog.Events;
using Shared.Extensions;
using Shared.Models.Results;

namespace Business.RequestHandlers.Auth;

public abstract class RefreshToken
{
    public class RefreshTokenRequest : IRequest<DataResult<Token>>
    {
        public string RefreshToken { get; set; } = default!;
    }

    public class RefreshTokenRequestHandler : IRequestHandler<RefreshTokenRequest, DataResult<Token>>
    {
        private readonly IUnitOfWork      _unitOfWork;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ILogger          _logger;

        private const string TokenIsNotValid = "Token is not valid";

        public RefreshTokenRequestHandler(IUnitOfWork unitOfWork, IJwtTokenService jwtTokenService, ILogger logger)
        {
            _unitOfWork      = unitOfWork;
            _jwtTokenService = jwtTokenService;
            _logger          = logger.ForContext("SourceContext", GetType().FullName);
        }


        public async Task<DataResult<Token>> Handle(RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var refreshToken = await _unitOfWork.UserTokens.GetTokenWithUserAsync(request.RefreshToken);

                if (refreshToken == null || refreshToken.User.IsDeleted)
                {
                    return DataResult<Token>.Invalid(TokenIsNotValid);
                }

                var newRefreshToken = new UserToken(TokenType.RefreshToken,
                    DateTime.UtcNow.AddDays(TokenConstants.RefreshTokenValidUntilDays), refreshToken.UserId);

                _unitOfWork.UserTokens.Remove(refreshToken);

                await _unitOfWork.UserTokens.AddAsync(newRefreshToken);

                await _unitOfWork.CommitAsync();

                var token = _jwtTokenService.CreateAccessToken(refreshToken.User, newRefreshToken.Token);

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
