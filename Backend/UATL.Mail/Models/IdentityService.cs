using UATL.MailSystem.Models;
using System.Security.Claims;
using System.Security.Principal;
using MongoDB.Entities;
using System.Net;
using Akavache;
using System.Reactive.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace UATL.MailSystem.Models
{
    public class IdentityService : IIdentityService
    {
        private IConfiguration _configuration;
        public IdentityService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private JwtSecurityToken ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var secret = _configuration["Jwt:Key"];
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

                var claimPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    RequireExpirationTime = true,
                    IssuerSigningKey = securityKey,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                return jwtToken;
            }
            catch(Exception ex)
            {
                return null;
            }

        }

        public async Task<Account?> GetCurrentAccount(HttpContext httpContext)
        {
            var token = httpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            try
            {
                var cached = await BlobCache.InMemory.GetObject<Account>(token);

                if (cached != null)
                    return cached;
            }
            catch { }

            var identity = httpContext.User.Identity as ClaimsIdentity;

            if (identity != null)
            { 
                try
                {

                    var accountId = identity.Claims.FirstOrDefault(o => o.Type == ClaimTypes.NameIdentifier)?.Value;
                    var username = identity.Claims.FirstOrDefault(o => o.Type == ClaimTypes.Email)?.Value;
                    var hash = identity.Claims.FirstOrDefault(o => o.Type == ClaimTypes.Hash)?.Value;
                    var role = identity.Claims.FirstOrDefault(o => o.Type == ClaimTypes.Role)?.Value;

                    var account = await DB.Find<Account>().MatchID(accountId).ExecuteSingleAsync().ConfigureAwait(false);
                    if (account.PasswordHash != hash || account.Role.ToString() != role || account.UserName != username) 
                        throw new Exception("Account Informations changed, Token Invalid! Please relogin.");

                    if (account != null) 
                        return account;
                    throw new Exception("Account not found! Token Invalid.");
                }
                catch(Exception ex)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await httpContext.Response.WriteAsJsonAsync(new { Message = ex.Message }).ConfigureAwait(false);
                    await httpContext.Response.CompleteAsync().ConfigureAwait(false);
                }
            }
            return null;
        }
    }
}
