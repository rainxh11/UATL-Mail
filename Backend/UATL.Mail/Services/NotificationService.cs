using Microsoft.AspNetCore.SignalR;
using UATL.Mail.Hubs;

namespace UATL.Mail.Services
{
    public class NotificationService
    {
        private IHubContext<MailHub> _mailHub;
        private readonly ILogger<MailHub> _logger;
        public NotificationService(IHubContext<MailHub> mailHub, ILogger<MailHub> logger)
        {
            _mailHub = mailHub;
            _logger = logger;
        }
        public async Task SendAll(string message, CancellationToken ct = default)
        {
            await _mailHub.Clients.All.SendAsync(message, ct);

        }
        public async Task Send(string userId, string message, CancellationToken ct = default)
        {
            await _mailHub.Clients.Group(userId).SendAsync(message, ct);
        }
    }
}
