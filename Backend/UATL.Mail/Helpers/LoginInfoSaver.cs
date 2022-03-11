using Ng.Services;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using Akavache;
using Hangfire;
using Newtonsoft.Json;
using UATL.Mail.Models;
using UATL.MailSystem.Models;

namespace UATL.Mail.Helpers
{
    public class LoginInfoSaver
    {
        private IUserAgentService _userAgentService;
        private IBackgroundJobClient _bgJobs;

        public LoginInfoSaver(IUserAgentService userAgentService, IBackgroundJobClient bgJobs)
        {
            _userAgentService = userAgentService;
            _bgJobs = bgJobs;
        }

        public async Task AddLogin(HttpContext context, Account account)
        {
            try
            {
                string userAgentString = context.Request.Headers["User-Agent"].ToString();
                UserAgent userAgent = _userAgentService.Parse(userAgentString);
                var accountLogin = new AccountLogin(account.ToBaseAccount(), userAgent, context);

                await BlobCache.LocalMachine.InsertObject(accountLogin.Id, accountLogin,
                    DateTimeOffset.Now.AddMonths(12));
            }
            catch(Exception ex)
            {
                var x = "";
            }
        } 
    }
}
