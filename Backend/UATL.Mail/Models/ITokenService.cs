
namespace UATL.Mail.Models
{
    public interface ITokenService
    {
        string BuildToken(IConfiguration config, Account account);
    }
}
