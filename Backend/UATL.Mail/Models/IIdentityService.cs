using UATL.MailSystem.Models;
using System.Security.Principal;
using Microsoft.AspNetCore.SignalR;

namespace UATL.MailSystem.Models
{
    public interface IIdentityService
    {
        Task<Account?> GetAccountFromToken(string token);
        Task<Account?> GetCurrentAccount(HttpContext httpContext);
        Task<Account?> GetCurrentHubClient(HubCallerContext httpContext);

    }
}
