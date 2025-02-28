using Infrastructure.Data.Postgres;
using Infrastructure.Data.Postgres.Entities;
using Infrastructure.Mail.Interface;
using MediatR;
using Microsoft.Extensions.Options;
using Shared.Models;
using Shared.Models.Configuration;
using Shared.Models.Event;

namespace Business.EventHandlers.Auth;

public abstract class ResetPasswordTokenCreated
{
    public class ResetPasswordTokenCreatedEvent : BaseEvent
    {
        public required string UserEmail { get; set; }
        public required int    UserId    { get; set; }
        public required string Token     { get; set; }
    }

    public class ResetPasswordTokenEventHandler : INotificationHandler<ResetPasswordTokenCreatedEvent>
    {
        private readonly IUnitOfWork                          _unitOfWork;
        private readonly IMailService                         _mailService;
        private readonly ConfigurationOptions.FrontAppOptions _frontAppOptions;

        private const string MailSubject = "Change password";
        private const string MailBody    = "You can change your password by clicking this link : {link}";

        public ResetPasswordTokenEventHandler(
            IUnitOfWork                                    unitOfWork,
            IMailService                                   mailService,
            IOptions<ConfigurationOptions.FrontAppOptions> frontAppOptions)
        {
            _unitOfWork      = unitOfWork;
            _mailService     = mailService;
            _frontAppOptions = frontAppOptions.Value;
        }

        public async Task Handle(ResetPasswordTokenCreatedEvent notification, CancellationToken cancellationToken)
        {
            var oldTokensToRemove = await _unitOfWork.UserTokens.FindAsync(t =>
                t.UserId    == notification.UserId && t.Token != notification.Token &&
                t.TokenType == TokenType.ResetPassword
            );

            _unitOfWork.UserTokens.RemoveRange(oldTokensToRemove);

            await _unitOfWork.CommitAsync();

            var mailBody = MailBody.Replace("{link}",
                $"{_frontAppOptions.Url}/change-password/{notification.Token}");

            await _mailService.SendMailAsync(MailSubject, mailBody, notification.UserEmail);
        }
    }
}
