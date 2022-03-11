using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using MongoDB.Entities;
using UATL.Mail.Helpers;
using UATL.MailSystem.Models;

namespace UATL.Mail
{
    public class BasicAuthMiddleware: IMiddleware
    {
        private readonly ILogger<BasicAuthMiddleware> _logger;
        private ITokenService _tokenService;
        private LoginInfoSaver _loginSaver;
        public BasicAuthMiddleware(ILogger<BasicAuthMiddleware> logger, ITokenService tokenService, LoginInfoSaver loginSaver)
        {
            _logger = logger;
            _tokenService = tokenService;
            _loginSaver = loginSaver;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.Path.StartsWithSegments("/backgroundjobs") || context.Request.Path.StartsWithSegments("/api"))
            {

                if (context.Request.Cookies.ContainsKey("T"))
                {
                    var token = context.Request.Cookies["T"];
                    context.Request.Headers["Authorization"] = $"Bearer {token}";
                }

                if (!context.Request.Headers["Authorization"].Any(x => x.Contains("Bearer")))
                {
                    try
                    {
                        var authHeader = AuthenticationHeaderValue.Parse(context.Request.Headers["Authorization"]);
                        var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                        var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
                        var username = credentials[0];
                        var password = credentials[1];

                        var account = await BasicAuthenticationHelper.Login(username, password).ConfigureAwait(false);
                        try
                        {
                            var accountModel = await DB.Find<Account>()
                                .MatchID(account.Claims.Single(x => x.Type == ClaimTypes.NameIdentifier).Value)
                                .ExecuteSingleAsync();

                            await _loginSaver.AddLogin(context, accountModel);
                        }
                        catch
                        {

                        }
                        

                        // authenticate credentials with user service and attach user to http context
                        context.User = account;
                        var token = _tokenService.BuildTokenFromIdentity(account.Identity);
                        context.Request.Headers["Authorization"] = $"Bearer {token}";

                        context.Response.Cookies.Append("T",token);
                        _logger.LogInformation("Account {0} Logged in!", username);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                        context.Response.StatusCode = 401;
                        context.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"\"");
                        await context.Response.CompleteAsync();
                    }
                }
            }
            await next(context);
        }
    }

    public class TokenInjectionMiddleware : IMiddleware
    {
        private readonly ILogger<BasicAuthMiddleware> _logger;
        private IIdentityService _identityService;
        public TokenInjectionMiddleware(ILogger<BasicAuthMiddleware> logger, IIdentityService identityService)
        {
            _logger = logger;
            _identityService = identityService;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.Query.ContainsKey("token"))
            {
                try
                {
                    var token = context.Request.Query["token"];
                    context.Request.Headers["Authorization"] = $"Bearer {token}";
                    /*var account = await _identityService.GetAccountFromToken(token);
                    if (account != null)
                    {
                        var claims = new[]
                        {
                            new Claim(ClaimTypes.NameIdentifier, account.ID),
                            new Claim(ClaimTypes.Role, account.Role.ToString()),
                            new Claim(ClaimTypes.Email, account.UserName),
                            new Claim(ClaimTypes.Hash, account.PasswordHash),
                        };
                        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims));
                    }*/
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }
            await next(context);
        }
    }
}
