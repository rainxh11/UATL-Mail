
using System.Security.Claims;
using System.Security.Principal;

namespace UATL.MailSystem.Models
{
    public interface ITokenService
    {
        ValueTask<string> BuildToken(IConfiguration config, Account account);
        string BuildTokenFromIdentity(IIdentity? identity);
    }
}
