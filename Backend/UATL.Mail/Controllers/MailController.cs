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
using System.Linq.Dynamic;
using System.Text.RegularExpressions;
using Hangfire.Storage.SQLite;

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
        public async Task<IActionResult> GetMails(int page = 1, int limit = 10, string? sort = "SentOn", bool desc = true,
            MailDirection? direction = MailDirection.Both,
            MailType? type = MailType.Internal)
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
                        query = query.Match(mail => mail.From.ID == account.ID || mail.To.ID == account.ID)
                            .Match(mail =>
                                (mail.AnyEq(x => x.Flags, MailFlag.Approved) &
                                 mail.Eq(x => x.Type, MailType.External)) |
                                mail.Eq(x => x.From.ID, account.ID) |
                                mail.Eq(x => x.Type, MailType.Internal));

                        break;
                    case MailDirection.Received:
                        query = query.Match(mail => mail.From.ID != account.ID && mail.To.ID == account.ID)
                            .Match(mail =>
                                (mail.AnyEq(x => x.Flags, MailFlag.Approved) &
                                 mail.Eq(x => x.Type, MailType.External)) | mail.Eq(x => x.Type, MailType.Internal));
                        break;
                    case MailDirection.Sent:
                        query = query.Match(mail => mail.From.ID == account.ID && mail.To.ID != account.ID);
                        break;
                }

                IEnumerable<MailFlag>? flags = null;
                var fluent = DB.Fluent<MailModel>().Match(x => x.AnyIn(x => x.Flags, flags));
                if (flags?.Count() > 0)
                    query = query.WithFluent(fluent);

                var mails = await query
                    .Match(x => x.Type == type || type == null)
                    .Sort(s => desc ? s.Descending(sort) : s.Ascending(sort))
                    .PageNumber(page)
                    .PageSize(limit < 0 ? int.MaxValue : limit)
                    .ExecuteAsync();

                return Ok(new PagedResultResponse<IEnumerable<MailModel>>(mails.Results, mails.TotalCount, mails.PageCount, limit, page));

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

                var draft = await DB.Find<MailModel>()
                    .Match(mail => (mail.Eq(x => x.From.ID, account.ID) | mail.Eq(x => x.To.ID, account.ID)) & 
                        (( mail.AnyEq(x => x.Flags, MailFlag.Approved) & mail.Eq(x => x.Type, MailType.External) ) | mail.Eq(x => x.Type, MailType.Internal))
                        )
                    .OneAsync(id);
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
        public async Task<IActionResult> SearchMails(int page = 1, int limit = 10, string? sort = "SentOn", bool desc = true, string search = "", 
            MailDirection? direction = MailDirection.Both,
            MailType? type = MailType.Internal
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
                        pipeline = pipeline.Match(mail => mail.From.ID == account.ID || mail.To.ID == account.ID)
                            .Match(mail =>
                                (mail.AnyEq(x => x.Flags, MailFlag.Approved) &
                                 mail.Eq(x => x.Type, MailType.External)) |
                                mail.Eq(x => x.From.ID, account.ID) |
                                mail.Eq(x => x.Type, MailType.Internal));
                        break;
                    case MailDirection.Received:
                        pipeline = pipeline.Match(mail => mail.From.ID != account.ID && mail.To.ID == account.ID)
                            .Match(mail =>
                                (mail.AnyEq(x => x.Flags, MailFlag.Approved) &
                                 mail.Eq(x => x.Type, MailType.External)) | mail.Eq(x => x.Type, MailType.Internal));
                        break;
                    case MailDirection.Sent:
                        pipeline = pipeline.Match(mail => mail.From.ID == account.ID && mail.To.ID != account.ID);
                        break;
                }

                var mails = await DB.PagedSearch<MailModel>()
                    .WithFluent(pipeline
                        .Match(x => x.Type == type || type == null)
                    )
                    .Sort(s => desc ? s.Descending(sort) : s.Ascending(sort))
                    .PageNumber(page)
                    .PageSize(limit < 0 ? int.MaxValue : limit)
                    .ExecuteAsync();

                return Ok(new PagedResultResponse<IEnumerable<MailModel>>(mails.Results, mails.TotalCount, mails.PageCount, limit, page));
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

                foreach (var file in files)
                {
                    var hash = HashHelper.CalculateFileFormMd5(file);
                    var query = DB.Find<Attachment>(transaction.Session).Match(x => x.MD5 == hash && x.FileSize == file.Length);
                    var exist = await query.ExecuteAnyAsync(ct);
                    if (exist)
                    {
                        var attachement = await query.ExecuteFirstAsync(ct);
                        await mail.Attachments.AddAsync(attachement, transaction.Session, ct);
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
                        await mail.Attachments.AddAsync(uploaded, transaction.Session, ct);
                    }
                }
                await mail.SaveAsync(transaction.Session);
                mail = await DB.Find<MailModel>(transaction.Session).OneAsync(mail.ID, ct);

                if (files.Count > 0)
                {
                    var attachments = await mail.Attachments.ChildrenFluent(transaction.Session).ToListAsync(ct);

                    foreach (var sent in mails.Where(x => x.ID != mail.ID))
                    {
                        await sent.SaveAsync(transaction.Session, ct);
                        await sent.Attachments.AddAsync(attachments, transaction.Session, ct);
                    }
                }
                else
                {
                    if (mails.Count(x => x.ID != mail.ID) > 0)
                    {
                        var bulkWrite = await DB.InsertAsync(mails.Where(x => x.ID != mail.ID), transaction.Session, ct);

                        if (!bulkWrite.IsAcknowledged)
                            return BadRequest();
                    }
                }

                await transaction.CommitAsync();


                var result = await DB.Find<MailModel>().ManyAsync(x => x.In(x => x.ID, mails.Select(i => i.ID)));

                return Ok(new ResultResponse<List<MailModel>, string>(result, $"Sent {result.Count + 1} Mails with {files.Count} attachements. To {value.Recipients.Count} Recipients."));
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
        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.User},{AccountRole.OrderOffice}")]
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

                var externalReceivedCount = DB.Queryable<MailModel>().Count(x => x.To.ID == account.ID && x.Type == MailType.External && x.Flags.Any(f => f == MailFlag.Approved));
                var externalUnreadReceivedCount = DB.Queryable<MailModel>().Count(x => x.To.ID == account.ID && x.Type == MailType.External && !x.Viewed && x.Flags.Any(f => f == MailFlag.Approved));

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

        //---------------------------------------------------------------------------------------------------------------------------------------------//
        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.User}")]
        [HttpGet]
        [Route("starred")]
        public async Task<IActionResult> GetStarred(bool full = false, int page = 1, int limit = 10, string ? sort = "SentOn", bool desc = true)
        {
            try
            {
                var account = await _identityService.GetCurrentAccount(HttpContext);
                page = page < 1 ? 0 : page - 1;

                if (full)
                {
                    var starredQuery = account.Starred;

                    var starred = starredQuery
                        .AsQueryable()
                        .OrderBy(sort)
                        .Skip(limit * page)
                        .Take(limit)
                        .ToList();

                    //return Ok(account.Starred);
                    return Ok(new PagedResultResponse<IEnumerable<MailModel>>(starred, 
                        starredQuery.Count(),
                        starredQuery.Count() % limit,
                        limit, 
                        page + 1));

                }
                else
                {
                    return Ok(account.Starred.Select(x => x.ID));
                }

            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest();
            }
        }

        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.User}")]
        [HttpGet]
        [Route("starred/search")]
        public async Task<IActionResult> SearchStarred(int page = 1, int limit = 10, string? sort = "SentOn", bool desc = true, string search = "")
        {
            try
            {
                var account = await _identityService.GetCurrentAccount(HttpContext);
                page = page < 1 ? 0 : page - 1;

                var query = new Func<MailModel, bool>(mail =>
                {
                    var regex = new Regex($"{search}", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                    return regex.IsMatch(mail.ID) ||
                           regex.IsMatch(mail.Subject) ||
                           regex.IsMatch(mail.Body) ||
                           regex.IsMatch(mail.From.Name) || regex.IsMatch(mail.From.UserName) ||
                           regex.IsMatch(mail.To.Name) || regex.IsMatch(mail.To.UserName) ||
                           mail.Attachments.Any(x => regex.IsMatch(x.Name)) ||
                           mail.HashTags.Any(x => regex.IsMatch(x));
                });

                var starredQuery = account.Starred
                    .Where(query);

                var starred = starredQuery
                    .AsQueryable()
                    .OrderBy(sort)
                    .Skip(limit * page)
                    .Take(limit)
                    .ToList();

                return Ok(new PagedResultResponse<IEnumerable<MailModel>>(starred, 
                    starredQuery.Count(),
                    starredQuery.Count() / limit.ToInt64(),
                    limit, 
                    page + 1));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest();
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------//

        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.User}")]
        [HttpPost]
        [Route("starred")]
        public async Task<IActionResult> AddStarred([FromBody] IEnumerable<string> ids, CancellationToken ct)
        {
            var transaction = DB.Transaction();
            try
            {
                var account = await _identityService.GetCurrentAccount(HttpContext);
                var haveAccess = await HaveAccessToMails(account, ct, ids.ToArray());
                if (!haveAccess)
                    return Unauthorized();

                await account.Starred.AddAsync(ids, transaction.Session, ct);
                account.ModifiedOn = DateTime.Now;
                await account.SaveAsync(transaction.Session, ct);

                await transaction.CommitAsync(ct);

                return Ok(account.Starred.Select(x => x.ID));
            }
            catch (Exception ex)
            {
                if (transaction.Session.IsInTransaction)
                    await transaction.AbortAsync();
                _logger.LogError(ex.Message);
                return BadRequest();
            }
        }

        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.User}")]
        [HttpPatch]
        [Route("starred")]
        public async Task<IActionResult> UpdateStarred([FromBody] IEnumerable<string> ids, CancellationToken ct)
        {
            var transaction = DB.Transaction();
            try
            {
                var account = await _identityService.GetCurrentAccount(HttpContext);
                var haveAccess = await HaveAccessToMails(account, ct, ids.ToArray());
                if (!haveAccess)
                    return Unauthorized();

                await account.Starred.RemoveAsync(account.Starred, transaction.Session, ct);
                await account.Starred.AddAsync(ids, transaction.Session, ct);
                account.ModifiedOn = DateTime.Now;
                await account.SaveAsync(transaction.Session, ct);
                await transaction.CommitAsync(ct);

                return Ok(account.Starred.Select(x => x.ID));
            }
            catch (Exception ex)
            {
                if (transaction.Session.IsInTransaction)
                    await transaction.AbortAsync();
                _logger.LogError(ex.Message);
                return BadRequest();
            }
        }

        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.User}")]
        [HttpDelete]
        [Route("starred/{id}")]
        public async Task<IActionResult> DeleteStarred(string id, CancellationToken ct)
        {
            var transaction = DB.Transaction();
            try
            {
                var account = await _identityService.GetCurrentAccount(HttpContext);
                var haveAccess = await HaveAccessToMails(account, ct, id);
                if (!haveAccess)
                    return Unauthorized();

                await account.Starred.RemoveAsync(id, transaction.Session, ct);
                account.ModifiedOn = DateTime.Now;
                await account.SaveAsync(transaction.Session, ct);
                await transaction.CommitAsync(ct);

                return Ok(account.Starred.Select(x => x.ID));
            }
            catch (Exception ex)
            {
                if (transaction.Session.IsInTransaction) 
                    await transaction.AbortAsync();
                _logger.LogError(ex.Message);
                return BadRequest();
            }
        }

        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.User}")]
        [HttpGet]
        [Route("{id}/replies")]
        public async Task<IActionResult> GetReplies(string id, CancellationToken ct)
        {
            try
            {
                var account = await _identityService.GetCurrentAccount(HttpContext);
                var haveAccess = await HaveAccessToMails(account, ct, id);
                if (!haveAccess)
                    return Unauthorized();

                var mail = await DB.Find<MailModel>().MatchID(id).ExecuteSingleAsync(ct);
                var replies = await DB.Find<MailModel>().ManyAsync(x => x.Eq(x => x.ReplyTo.ID, id), ct);


                return Ok(new {Mail = mail, Replies = replies.OrderByDescending(x => x.SentOn)});
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest();
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------//
        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.User}")]
        [HttpGet]
        [Route("tags")]
        public async Task<IActionResult> GetTags(string? search = "")
        {
            try
            {
                var account = await _identityService.GetCurrentAccount(HttpContext);
                var regex = new Regex(search, RegexOptions.IgnoreCase | RegexOptions.Singleline);

                var tags = await DB.Fluent<MailModel>()
                    .Match(mail => mail.Eq(x => x.From.ID, account.ID) | mail.Eq(x => x.To.ID, account.ID))
                    .ToListAsync();

                var matchedTags = tags
                    .SelectMany(x => x.HashTags)
                    .Where(x => regex.IsMatch(x))
                    .ToList();

                return Ok(matchedTags.Distinct());

            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------//

        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.User}")]
        [HttpGet]
        [Route("tagged")]
        public async Task<IActionResult> GetTaggedMails(string tag, int page = 1, int limit = 10, string? sort = "SentOn", bool desc = true)
        {
            try
            {
                sort = FirstCharToUpper(sort.Replace("-", "").Trim());

                var account = await _identityService.GetCurrentAccount(HttpContext);
                var query = DB.PagedSearch<MailModel, MailModel>();

                var fluent = DB.Fluent<MailModel>().Match(mail => mail.AnyEq(x => x.HashTags, tag) & ( mail.Eq(x => x.From.ID, account.ID) | mail.Eq(x => x.To.ID, account.ID)) );

                var mails = await query
                    .WithFluent(fluent)
                    .Sort(s => desc ? s.Descending(sort) : s.Ascending(sort))
                    .PageNumber(page)
                    .PageSize(limit < 0 ? int.MaxValue : limit)
                    .ExecuteAsync();

                return Ok(new PagedResultResponse<IEnumerable<MailModel>>(mails.Results, mails.TotalCount, mails.PageCount, limit, page));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.User}")]
        [HttpGet]
        [Route("tagged/search")]
        public async Task<IActionResult> SearchTaggedMails(string tag, string? search = "", int page = 1, int limit = 10, string? sort = "SentOn", bool desc = true)
        {
            try
            {
                sort = FirstCharToUpper(sort.Replace("-", "").Trim());

                var account = await _identityService.GetCurrentAccount(HttpContext);

                var pipeline = DB.FluentTextSearch<MailModel>(Search.Full, search)
                    .Match(mail => mail.AnyEq(x => x.HashTags, tag) & (mail.Eq(x => x.From.ID, account.ID) | mail.Eq(x => x.To.ID, account.ID)));

                var mails = await DB.PagedSearch<MailModel>()
                    .WithFluent(pipeline)
                    .Sort(s => desc ? s.Descending(sort) : s.Ascending(sort))
                    .PageNumber(page)
                    .PageSize(limit < 0 ? int.MaxValue : limit)
                    .ExecuteAsync();

                return Ok(new PagedResultResponse<IEnumerable<MailModel>>(mails.Results, mails.TotalCount, mails.PageCount, limit, page));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------//
        private async Task<bool> HaveAccessToMails(Account account, CancellationToken ct, params string[] ids)
        {
            var transaction = DB.Transaction();
            try
            {
                int count = 0;

                if (account.Role == AccountType.OrderOffice)
                {
                    var mails = await DB.Find<MailModel>(session: transaction.Session)
                        .ManyAsync(filter => filter.In(x => x.ID, ids) & filter.Eq(x => x.Type, MailType.External), ct);
                    count = mails.Count;
                }
                else
                {
                     var mails = await DB.Find<MailModel>(session: transaction.Session)
                        .ManyAsync(filter => filter.In(x => x.ID, ids)
                                             & (filter.Eq(x => x.From.ID, account.ID) | filter.Eq(x => x.To.ID, account.ID)), ct);
                     count = mails.Count;
                }

                await transaction.CommitAsync(ct);
                return count == ids.Count();
            }
            catch (Exception ex)
            {
                if (transaction.Session.IsInTransaction)
                    await transaction.AbortAsync();
                _logger.LogError(ex.Message);
                return false;
            }
        }
    }
}
