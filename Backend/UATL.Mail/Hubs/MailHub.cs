using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace UATL.Mail.Hubs
{
    [Authorize]
    public class MailHub : Hub
    {
        public async Task SendAll(string message, string data)
        {
            await Clients.All.SendAsync(message, data);

        }
    }
    [Authorize]
    public class ChatHub : Hub
    {

    }
}
