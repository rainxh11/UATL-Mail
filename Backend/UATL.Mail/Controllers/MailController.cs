using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using UATL.MailSystem.Models;
using UATL.MailSystem.Models.Models;
using UATL.MailSystem.Models.Response;
using System.Linq.Dynamic;
using System.Linq.Dynamic.Core;
using UATL.MailSystem.Models.Models.Request;
using Mapster.Adapters;
using Mapster;
using MongoDB.Driver;
using System.Linq.Expressions;
using MongoDB.Bson;
using System.Linq;
using System.Xml.Linq;
using UATL.Mail.Models.Bindings;
using FluentValidation.AspNetCore;
using UATL.Mail.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UATL.Mail.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class MailController : ControllerBase
    {
        public static string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input))
                return String.Empty;
            return input.First().ToString().ToUpper() + input.Substring(1);
        }

        private readonly ILogger<MailController> _logger;
        private IConfiguration _config;
        private ITokenService _tokenService;
        private IIdentityService _identityService;
        public MailController(
            ILogger<MailController> logger,
            IIdentityService identityService,
            ITokenService tokenService,
            IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _tokenService = tokenService;
            _identityService = identityService;
        }


        //---------------------------------------------------------------------------------------------------------------------------------------------//
        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.User}")]
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetMails(int page = 1, int limit = 10, string? sort = "CreatedOn", bool desc = true, 
            [JsonProperty(ItemConverterType = typeof(StringEnumConverter))] MailDirection direction = MailDirection.Both,
            [JsonProperty(ItemConverterType = typeof(StringEnumConverter))] MailType? type = MailType.Internal
            )
        {
            try
            {
                sort = FirstCharToUpper(sort.Replace("-", "").Trim());

                var account = await _identityService.GetCurrentAccount(HttpContext);
                var query = DB.PagedSearch<MailModel, MailModel>();

                switch (direction)
                {
                    default:
                    case MailDirection.Both:
                        query = query.Match(mail => mail.From.ID == account.ID || mail.To.ID == account.ID);
                        break;
                    case MailDirection.Received:
                        query = query.Match(mail => mail.From.ID != account.ID && mail.To.ID == account.ID);
                        break;
                    case MailDirection.Sent:
                        query = query.Match(mail => mail.From.ID == account.ID && mail.To.ID != account.ID);
                        break;
                }

                var mails = await query
                    .Match(x => x.Type == type || type == null)
                    .Sort(s => desc ? s.Descending(sort) : s.Ascending(sort))
                    .PageNumber(page)
                    .PageSize(limit < 0 ? int.MaxValue : limit)
                    .ExecuteAsync();

                return Ok(new ResultResponse<IEnumerable<MailModel>, long>(mails.Results, mails.TotalCount)); ;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------//
        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.User}")]
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetMail(string id)
        {
            try
            {
                var account = await _identityService.GetCurrentAccount(HttpContext);

                var draft = await DB.Find<MailModel>().Match(mail => mail.From.ID == account.ID || mail.To.ID == account.ID).OneAsync(id);
                if (draft == null) return NotFound();

                return Ok(draft); ;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------//

        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.User}")]
        [HttpGet]
        [Route("search")]
        public async Task<IActionResult> SearchMails(int page = 1, int limit = 10, string? sort = "CreatedOn", bool desc = true, string search = "", 
            [JsonProperty(ItemConverterType = typeof(StringEnumConverter))] MailDirection direction = MailDirection.Both,
            [JsonProperty(ItemConverterType = typeof(StringEnumConverter))] MailType? type = MailType.Internal
        )
        {
            try
            {
                sort = FirstCharToUpper(sort.Replace("-", "").Trim());

                var account = await _identityService.GetCurrentAccount(HttpContext);

                var pipeline = DB.FluentTextSearch<MailModel>(Search.Full, search);

                switch (direction)
                {
                    default:
                    case MailDirection.Both:
                        pipeline = pipeline.Match(mail => mail.From.ID == account.ID || mail.To.ID == account.ID);
                        break;
                    case MailDirection.Received:
                        pipeline = pipeline.Match(mail => mail.From.ID != account.ID && mail.To.ID == account.ID);
                        break;
                    case MailDirection.Sent:
                        pipeline = pipeline.Match(mail => mail.From.ID == account.ID && mail.To.ID != account.ID);
                        break;
                }

                var drafts = await DB.PagedSearch<MailModel>()
                    .WithFluent(pipeline.Match(x => x.Type == type || type == null))
                    .Sort(s => desc ? s.Descending(sort) : s.Ascending(sort))
                    .PageNumber(page)
                    .PageSize(limit < 0 ? int.MaxValue : limit)
                    .ExecuteAsync();

                return Ok(new ResultResponse<IEnumerable<MailModel>, long>(drafts.Results, drafts.TotalCount)); ;
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------//
        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.User}")]
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> SendMails([ModelBinder(BinderType = typeof(JsonModelBinder))] SendMailRequest value, IList<IFormFile>? files, CancellationToken ct)
        {
            var transaction = DB.Transaction();
            try
            {
                var account = await _identityService.GetCurrentAccount(HttpContext);
                var mails = await MailRequestHelper.GetMails(value, account, ct, transaction.Session);
                var mail = mails.First();

                await mail.InsertAsync(transaction.Session, ct);
                mail = await DB.Find<MailModel>(transaction.Session).Match(x => x.From.ID == account.ID).OneAsync(mail.ID, ct);
                if (mail == null)
                    return NotFound();

                var attachements = new List<Attachment>();
                foreach (var file in files)
                {
                    var hash = HashHelper.CalculateFileFormMd5(file);
                    var query = DB.Find<Attachment>(transaction.Session).Match(x => x.MD5 == hash && x.FileSize == file.Length);
                    var exist = await query.ExecuteAnyAsync(ct);
                    if (exist)
                    {
                        var attachement = await query.ExecuteFirstAsync(ct);
                        mail.Attachements.Add(attachement);
                    }
                    else
                    {
                        var attachement = new Attachment()
                        {
                            MD5 = hash,
                            UploadedBy = account.ToBaseAccount(),
                            ContentType = file.ContentType,
                            Name = file.FileName,
                        };
                        await attachement.SaveAsync(transaction.Session);
                        using (var stream = file.OpenReadStream())
                        {
                            await attachement.Data.UploadAsync(stream, cancellation: ct, session: transaction.Session);
                        }
                        var uploaded = await DB.Find<Attachment>(transaction.Session).OneAsync(attachement.ID);
                        mail.Attachements.Add(uploaded);
                    }
                }
                await mail.SaveAsync(transaction.Session);
                mail = await DB.Find<MailModel>(transaction.Session).OneAsync(mail.ID, ct);

                mails = mails
                    .Where(x => x.ID != mail.ID)
                    .Select(x =>
                    {
                        x.Attachements = mail.Attachements;

                        return x;
                    })
                    .ToList();

                if(mails.Count != 0 && mails != null)
                {
                    var bulkWrite = await DB.InsertAsync<MailModel>(mails, transaction.Session, ct);

                    await transaction.CommitAsync();
                    if (!bulkWrite.IsAcknowledged)
                        return BadRequest();
                }


                var result = await DB.Find<MailModel>().ManyAsync(x => x.In(x => x.ID, mails.Select(i => i.ID)));

                return Ok(new ResultResponse<List<MailModel>, string>(result, $"Sent {result.Count} Mails with {files.Count} attachements. To {value.Recipients.Count} Recipients."));
            }
            catch (Exception ex)
            {
                if(transaction.Session.IsInTransaction) 
                    await transaction.AbortAsync();
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------//
        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.User}")]
        [HttpGet]
        [Route("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var account = await _identityService.GetCurrentAccount(HttpContext);

                var internalReceivedCount = DB.Queryable<MailModel>().Count(x => x.To.ID == account.ID && x.Type == MailType.Internal);
                var internalUnreadReceivedCount = DB.Queryable<MailModel>().Count(x => x.To.ID == account.ID && x.Type == MailType.Internal && !x.Viewed);

                var internalSentCount = DB.Queryable<MailModel>().Count(x => x.From.ID == account.ID && x.Type == MailType.Internal);

                var externalReceivedCount = DB.Queryable<MailModel>().Count(x => x.To.ID == account.ID && x.Type == MailType.External);
                var externalUnreadReceivedCount = DB.Queryable<MailModel>().Count(x => x.To.ID == account.ID && x.Type == MailType.External && !x.Viewed);

                var externalSentCount = DB.Queryable<MailModel>().Count(x => x.From.ID == account.ID && x.Type == MailType.External);

                var starred = await DB.Find<MailModel>()
                    .Match(mail =>
                        mail.In(x => x.ID, account.Starred.Select(z => z.ID)) &
                        (mail.Eq(x => x.From.ID, account.ID) | mail.Eq(x => x.To.ID, account.ID)))
                    .ExecuteAsync();

                var draftCount = DB.Queryable<Draft>().Count(x => x.From.ID == account.ID);

                return Ok(new
                {
                    InternalReceived = new {Count = internalReceivedCount, Unread = internalUnreadReceivedCount},
                    ExternalReceived = new {Count = externalReceivedCount, Unread = externalUnreadReceivedCount},
                    InternalSent = new {Count = internalSentCount},
                    ExternalSent = new {Count = externalSentCount},
                    Drafts = new {Count = draftCount},
                    Starred = new {Count = starred.Count}
                });

            }
            catch(Exception ex) 
            {
                _logger.LogError(ex.Message);
                return BadRequest();
            }
        }

    }
}
