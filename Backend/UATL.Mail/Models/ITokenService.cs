
namespace UATL.MailSystem.Models
{
    public interface ITokenService
    {
        string BuildToken(IConfiguration config, Account account);
    }
}
