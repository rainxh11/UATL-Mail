using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using UATL.MailSystem.Models;

namespace UATL.Mail.Hubs
{
    [Authorize]
    public class MailHub : Hub
    {
        private readonly ILogger<MailHub> _logger;

        public MailHub(ILogger<MailHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var user = Context.User;

            _logger.LogInformation("Hub Client: {0} Connected. {1}", userId, user);
            await Groups.AddToGroupAsync(Context.ConnectionId, userId);
            await base.OnConnectedAsync();
        }
    }
    [Authorize]
    public class ChatHub : Hub
    {

    }
}
