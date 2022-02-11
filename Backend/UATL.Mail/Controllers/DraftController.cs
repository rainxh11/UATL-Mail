using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using UATL.Mail.Models;
using UATL.Mail.Models.Models;
using UATL.Mail.Models.Response;
using System.Linq.Dynamic;
using System.Linq.Dynamic.Core;
using UATL.Mail.Models.Models.Request;
using Mapster.Adapters;
using Mapster;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace UATL.Mail.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class DraftController : Controller
    {
        public static string FirstCharToUpper(string input)
        {
            if (String.IsNullOrEmpty(input))
                return String.Empty;
            return input.First().ToString().ToUpper() + input.Substring(1);
        }

        private readonly ILogger<DraftController> _logger;
        private IConfiguration _config;
        private ITokenService _tokenService;
        private IIdentityService _identityService;
        public DraftController(
            ILogger<DraftController> logger,
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
        public async Task<IActionResult> GetDrafts(int page = 1, int limit = 10, string? sort = "CreatedOn", bool desc = true)
        {
            try
            {
                sort = FirstCharToUpper(sort.Replace("-", "").Trim());

                var account = await _identityService.GetCurrentAccount(HttpContext.User.Identity);

                var drafts = await DB.PagedSearch<Draft>()
                    .Match(draft => draft.From.ID == account.ID)
                    .Sort(s => desc ? s.Descending(sort) : s.Ascending(sort))
                    .PageNumber(page)
                    .PageSize(limit)
                    .ExecuteAsync();

                return Ok(new ResultResponse<IEnumerable<Draft>, long>(drafts.Results, drafts.TotalCount));;
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------//

        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.User}")]
        [HttpGet]
        [Route("search")]
        public async Task<IActionResult> SearchDrafts(int page = 1, int limit = 10, string? sort = "CreatedOn", bool desc = true, string search = "")
        {
            try
            {
                sort = FirstCharToUpper(sort.Replace("-", "").Trim());

                var account = await _identityService.GetCurrentAccount(HttpContext.User.Identity);

                var pipeline = DB.FluentTextSearch<Draft>(Search.Full, search).Match(draft => draft.From.ID == account.ID);

                var drafts = await DB.PagedSearch<Draft>()
                    .WithFluent(pipeline)
                    .Sort(s => desc ? s.Descending(sort) : s.Ascending(sort))
                    .PageNumber(page)
                    .PageSize(limit)
                    .ExecuteAsync();

                return Ok(new ResultResponse<IEnumerable<Draft>, long>(drafts.Results, drafts.TotalCount)); ;
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
        public async Task<IActionResult> AddDraft([FromBody] DraftRequest draftRequest)
        {
            try
            {
                var account = await _identityService.GetCurrentAccount(HttpContext.User.Identity);

                var draft = draftRequest.Adapt<Draft>();
                draft.From = account;

                await draft.InsertAsync();
                var result = await DB.Find<Draft>().OneAsync(draft.ID);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------//
        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.User}")]
        [HttpPatch]
        [Route("{id}")]
        public async Task<IActionResult> UpdateDraft(string id, [FromBody] DraftRequest draftRequest)
        {
            try
            {
                var account = await _identityService.GetCurrentAccount(HttpContext.User.Identity);
                var draft = await DB.Find<Draft>().OneAsync(id);

                draft.Body = draftRequest.Body;
                draft.Subject = draftRequest.Subject;

                await draft.SaveAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------//
        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.User}")]
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteDraft(string id)
        {
            try
            {
                var account = await _identityService.GetCurrentAccount(HttpContext.User.Identity);

                var result = await DB.DeleteAsync<Draft>(x => x.From.ID == account.ID && x.ID == id);

                if (!result.IsAcknowledged)
                {
                    return Unauthorized();
                }
                else
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------//
        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.User}")]
        [HttpDelete]
        [Route("")]
        public async Task<IActionResult> DeleteManyDraft([FromBody] IEnumerable<string> ids)
        {
            try
            {
                var account = await _identityService.GetCurrentAccount(HttpContext.User.Identity);

                var result = await DB.DeleteAsync<Draft>(x => x.From.ID == account.ID && ids.Contains(x.ID));

                if (!result.IsAcknowledged)
                {
                    return Unauthorized();
                }
                else
                {
                    return Ok($"Deleted {result.DeletedCount} Items.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        //---------------------------------------------------------------------------------------------------------------------------------------------//
    }
}
