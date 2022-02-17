
namespace UATL.MailSystem.Models
{
    public interface ITokenService
    {
        ValueTask<string> BuildToken(IConfiguration config, Account account);
    }
}
