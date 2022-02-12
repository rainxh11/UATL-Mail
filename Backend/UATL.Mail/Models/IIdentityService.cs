using UATL.MailSystem.Models;
using System.Security.Principal;

namespace UATL.MailSystem.Models
{
    public interface IIdentityService
    {
        Task<Account?> GetCurrentAccount(HttpContext httpContext);
    }
}
