using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Entities;
using UATL.MailSystem.Models;
using UATL.MailSystem.Models.Models;
using UATL.MailSystem.Models.Response;

namespace UATL.Mail.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        public static string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input))
                return String.Empty;
            return input.First().ToString().ToUpper() + input.Substring(1);
        }

        private readonly ILogger<OrderController> _logger;
        private IConfiguration _config;
        private ITokenService _tokenService;
        private IIdentityService _identityService;
        public OrderController(
            ILogger<OrderController> logger,
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
        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.OrderOffice}")]
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetMails(int page = 1, int limit = 10, string? sort = "SentOn", bool desc = true)
        {
            try
            {
                sort = FirstCharToUpper(sort.Replace("-", "").Trim());

                var mails = await DB.PagedSearch<MailModel>()
                    .Match(x => x.Type == MailType.External)
                    .ProjectExcluding(x => new { x.Body, x.Attachments })
                    .Sort(s => desc ? s.Descending(sort) : s.Ascending(sort))
                    .PageNumber(page)
                    .PageSize(limit < 0 ? int.MaxValue : limit)
                    .ExecuteAsync();

                return Ok(new PagedResultResponse<IEnumerable<MailModel>>(
                    mails.Results.Select(x =>
                    {
                        x.Body = null;
                        x.Attachments = null;
                        return x;
                    }),
                    mails.TotalCount, 
                    mails.PageCount, 
                    limit, 
                    page));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------//
        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.OrderOffice}")]
        [HttpGet]
        [Route("search")]
        public async Task<IActionResult> SearchMails(int page = 1, int limit = 10, string? sort = "SentOn", bool desc = true, string search = "")
        {
            try
            {
                sort = FirstCharToUpper(sort.Replace("-", "").Trim());

                var pipeline = DB.FluentTextSearch<MailModel>(Search.Full, search);

                var mails = await DB.PagedSearch<MailModel>()
                    .WithFluent(pipeline.Match(x => x.Type == MailType.External))
                    .ProjectExcluding(x => new { x.Body, x.Attachments })
                    .Sort(s => desc ? s.Descending(sort) : s.Ascending(sort))
                    .PageNumber(page)
                    .PageSize(limit < 0 ? int.MaxValue : limit)
                    .ExecuteAsync();

                return Ok(new PagedResultResponse<IEnumerable<MailModel>>(
                    mails.Results.Select(x =>
                    {
                        x.Body = null;
                        x.Attachments = null;
                        return x;
                    }),
                    mails.TotalCount,
                    mails.PageCount,
                    limit,
                    page));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //------------------------------------------------------------------------------------------------------------//
        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.OrderOffice}")]
        [HttpPatch]
        [Route("{id}/approve")]
        public async Task<IActionResult> ApproveExternalMail(string id, CancellationToken ct)
        {
            var transaction = DB.Transaction();
            try
            {
                var account = await _identityService.GetCurrentAccount(HttpContext);
                var update = await DB.Find<MailModel>(transaction.Session)
                    .MatchID(id)
                    .ExecuteSingleAsync(ct);
                if (update == null)
                    return NotFound();

                if (update.Approved)
                    return BadRequest();

                update.Flags = update.Flags.Append(MailFlag.Approved).Append(MailFlag.Reviewed).Distinct().ToList();
                update.ApprovedBy = account.ToBaseAccount();

                await update.SaveAsync(transaction.Session, ct);
                await transaction.CommitAsync();

                return Ok();

            }
            catch(Exception ex)
            {
                if (transaction.Session.IsInTransaction)
                    await transaction.AbortAsync();
                _logger.LogError(ex.Message);
                return BadRequest();
            }
        }
        //------------------------------------------------------------------------------------------------------------//
        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.OrderOffice}")]
        [HttpPatch]
        [Route("{id}/review")]
        public async Task<IActionResult> ReviewOrder(string id, CancellationToken ct)
        {
            var transaction = DB.Transaction();
            try
            {
                var account = await _identityService.GetCurrentAccount(HttpContext);
                var update = await DB.Find<MailModel>(transaction.Session)
                    .MatchID(id)
                    .ExecuteSingleAsync(ct);

                if (update == null)
                    return NotFound();

                update.Flags = update.Flags.Append(MailFlag.Reviewed).Distinct().ToList();

                await update.SaveAsync(transaction.Session, ct);
                await transaction.CommitAsync();

                return Ok();

            }
            catch (Exception ex)
            {
                if (transaction.Session.IsInTransaction)
                    await transaction.AbortAsync();
                _logger.LogError(ex.Message);
                return BadRequest();
            }
        }
    }
}
