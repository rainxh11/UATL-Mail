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
        public async Task<IActionResult> GetMails(int page = 1, int limit = 10, string? sort = "CreatedOn", bool desc = true, [JsonProperty(ItemConverterType = typeof(StringEnumConverter))] MailDirection type = MailDirection.Both)
        {
            try
            {
                sort = FirstCharToUpper(sort.Replace("-", "").Trim());

                var account = await _identityService.GetCurrentAccount(HttpContext);
                var query = DB.PagedSearch<MailModel, MailModel>();

                switch (type)
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
        public async Task<IActionResult> SearchMails(int page = 1, int limit = 10, string? sort = "CreatedOn", bool desc = true, string search = "", [JsonProperty(ItemConverterType = typeof(StringEnumConverter))] MailDirection type = MailDirection.Both)
        {
            try
            {
                sort = FirstCharToUpper(sort.Replace("-", "").Trim());

                var account = await _identityService.GetCurrentAccount(HttpContext);

                var pipeline = DB.FluentTextSearch<MailModel>(Search.Full, search);

                switch (type)
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
                    .WithFluent(pipeline)
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
    }
}
