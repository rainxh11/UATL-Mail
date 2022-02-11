using UATL.Mail.Models;
using System.Security.Claims;
using System.Security.Principal;
using MongoDB.Entities;

namespace UATL.Mail.Models
{
    public class IdentityService : IIdentityService
    {
        public async Task<Account?> GetCurrentAccount(HttpContext httpContext)
        {
            var identity = httpContext.User.Identity as ClaimsIdentity;

            if (identity != null)
            { 
                try
                {
                    var accountId = identity.Claims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value;
                    return await DB.Find<Account>().MatchID(accountId).ExecuteSingleAsync();
                }
                catch
                {
                    await httpContext.Response.WriteAsync("Unauthorized Access / User claim expired / User Account doesn't exist!");
                    await httpContext.Response.CompleteAsync();
                }
            }
            return null;
        }
    }
}
