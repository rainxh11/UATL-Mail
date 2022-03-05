using System.Net.Http.Headers;
using System.Text;
using UATL.Mail.Helpers;
using UATL.MailSystem.Models;

namespace UATL.Mail
{
    public class BasicAuthMiddleware: IMiddleware
    {
        private readonly ILogger<BasicAuthMiddleware> _logger;
        private ITokenService _tokenService;
        public BasicAuthMiddleware(ILogger<BasicAuthMiddleware> logger, ITokenService tokenService)
        {
            _logger = logger;
            _tokenService = tokenService;
        }
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.Path.StartsWithSegments("/api") || context.Request.Path.StartsWithSegments("/backgroundjobs") )
            {
                

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

                        // authenticate credentials with user service and attach user to http context
                        context.User = account;
                        var token = _tokenService.BuildTokenFromIdentity(account.Identity);

                        context.Response.Cookies.Append("T",token);
                        _logger.LogInformation("Account {0} Logged in!", username);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                        /*context.Response.StatusCode = 401;
                        context.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"\"");
                        await context.Response.CompleteAsync();*/
                    }
                }
            }
            await next(context);
        }
    }
}
