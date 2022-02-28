using FluentEmail.Core;
using Microsoft.AspNetCore.SignalR;
using UATL.Mail.Hubs;

namespace UATL.Mail.Services
{
    public class NotificationService
    {
        private IHubContext<MailHub> _mailHub;
        private readonly ILogger<MailHub> _logger;
        private IFluentEmail _fluentMail;

        public NotificationService(IHubContext<MailHub> mailHub, ILogger<MailHub> logger, IFluentEmail fluentMail)
        {
            _mailHub = mailHub;
            _logger = logger;
            _fluentMail = fluentMail;
        }
        public async Task SendAll(string message, CancellationToken ct = default)
        {
            await _mailHub.Clients.All.SendAsync(message, ct);

        }
        public async Task Send(string userId, string message, CancellationToken ct = default)
        {
            await _mailHub.Clients.Group(userId).SendAsync(message, ct);
        }

        public async Task SendEmail(string to, string subject, string message, CancellationToken ct = default)
        {
             await _fluentMail
                .To(to)
                .Subject(subject)
                .SendAsync(ct);
        }
    }
}
