using UATL.Mail.Models;
using System.Security.Claims;
using System.Security.Principal;
using MongoDB.Entities;

namespace UATL.Mail.Models
{
    public class IdentityService : IIdentityService
    {
        public async Task<Account?> GetCurrentAccount(IIdentity? _identity)
        {
            var identity = _identity as ClaimsIdentity;

            if (identity != null)
            { 
                try
                {
                    var accountId = identity.Claims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value;
                    return await DB.Find<Account>().MatchID(accountId).ExecuteSingleAsync();
                }
                catch
                {
                }
            }
            return null;
        }
    }
}
