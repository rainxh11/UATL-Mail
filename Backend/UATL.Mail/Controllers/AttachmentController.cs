using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Net.Http.Headers;
using MongoDB.Entities;
using UATL.Mail.Hubs;
using UATL.MailSystem.Models;
using UATL.MailSystem.Models.Models;

namespace UATL.Mail.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AttachmentController : ControllerBase
    {
        private readonly ILogger<AttachmentController> _logger;
        private IConfiguration _config;
        private ITokenService _tokenService;
        private IIdentityService _identityService;
        private readonly IHubContext<MailHub> _mailHub;
        public AttachmentController(
            ILogger<AttachmentController> logger,
            IIdentityService identityService,
            ITokenService tokenService,
            IHubContext<MailHub> mailHub,
            IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _tokenService = tokenService;
            _identityService = identityService;
            _mailHub = mailHub;
        }

        //[Authorize(Roles = $"{AccountRole.Admin},{AccountRole.User}")]
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> DownloadAttachment(string id, CancellationToken ct)
        {
            try
            {
                var account = await _identityService.GetCurrentAccount(HttpContext);
                if (account == null)
                {
                    HttpContext.Response.Headers.Add("WWW-Authenticate", "Basic realm=\"\"");
                    await HttpContext.Response.CompleteAsync();
                }

                var allowed = await HaveAccessToFile(id, account, ct);

                if (!allowed)
                    return Unauthorized();

                var attachment = await DB.Find<Attachment>().OneAsync(id, ct);
                if (attachment == null)
                    return NotFound();


                var stream = new MemoryStream();
                await attachment.Data.DownloadAsync(stream, cancellation: ct).ConfigureAwait(false);
                stream.Position = 0;
                this.HttpContext.Response.ContentLength = attachment.FileSize;

                var contentType = string.IsNullOrEmpty(attachment.ContentType) ?  "application/octet-stream" : attachment.ContentType;

                return File(stream, contentType, attachment.Name, true);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest();
            }
        }
        ///---------------------------------------------------------------------------------------------------------///
        private async Task<bool> HaveAccessToFile(string id, Account account, CancellationToken ct)
        {
            var transaction = DB.Transaction();
            try
            {
                var drafts = DB.Find<Draft>(session: transaction.Session)
                    .ManyAsync(filter => filter.ElemMatch(x => x.Attachments, x => x.ID == id) & filter.Eq(x => x.From.ID, account.ID), ct);

                var mails = DB.Find<MailModel>(session: transaction.Session)
                    .ManyAsync(filter => filter.ElemMatch(x => x.Attachments, x => x.ID == id) & ( filter.Eq(x => x.From.ID, account.ID) | filter.Eq(x => x.To.ID, account.ID) ), ct);

                var attachments = DB.Find<Attachment>(session: transaction.Session)
                    .ManyAsync(filter => filter.Eq(x => x.UploadedBy.ID, account.ID));

                await Task.WhenAll(drafts, mails, attachments);
                await transaction.CommitAsync(ct);

                return drafts.Result?.Count > 0 || mails.Result?.Count > 0 || attachments.Result?.Count > 0;
            }
            catch(Exception ex)
            {
                if (transaction.Session.IsInTransaction)
                    await transaction.AbortAsync();
                _logger.LogError(ex.Message);
                return false;
            }
        }
    }
}
