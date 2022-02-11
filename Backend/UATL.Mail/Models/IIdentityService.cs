using UATL.Mail.Models;
using System.Security.Principal;

namespace UATL.Mail.Models
{
    public interface IIdentityService
    {
        Task<Account?> GetCurrentAccount(HttpContext httpContext);
    }
}
