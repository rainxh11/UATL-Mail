using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UATL.MailSystem.Models;
using Jetsons.JetPack;
using Akavache;
using System.Reactive.Linq;

namespace UATL.MailSystem.Models
{
    public class TokenService : ITokenService
    {
        private IConfiguration _configuration;
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async ValueTask<string> BuildToken(IConfiguration config, Account account)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

            var claims = new[]
{
                new Claim(ClaimTypes.NameIdentifier, account.ID),
                new Claim(ClaimTypes.Role, account.Role.ToString()),
                new Claim(ClaimTypes.Email, account.UserName),
                new Claim(ClaimTypes.Hash, account.PasswordHash),
            };
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                //Expires = DateTime.Now.AddHours(config["Jwt:ExpireAfter"].ToInt()),
                Expires = DateTime.Now.AddHours(config["Jwt:ExpireAfter"].ToInt()),
                SigningCredentials = credentials
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            var tokenString = tokenHandler.WriteToken(token);
            await BlobCache.InMemory.InsertObject<Account>(tokenString, account, DateTime.Now.AddHours(config["Jwt:ExpireAfter"].ToInt()));
            //await BlobCache.LocalMachine.InsertObject<Account>(tokenString, account, TimeSpan.FromSeconds(30));


            return tokenString;
        }

    }
}
