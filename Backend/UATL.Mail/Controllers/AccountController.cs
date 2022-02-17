using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UATL.MailSystem.Models.Request;
using UATL.MailSystem.Models;
using Microsoft.AspNetCore.Authorization;
using UATL.MailSystem.Models.Response;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Entities;

namespace UATL.MailSystem.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;
        private IConfiguration _config;
        private ITokenService _tokenService;
        private IIdentityService _identityService;
        public AccountController(
            ILogger<AccountController> logger,
            IIdentityService identityService,
            ITokenService tokenService,
            IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _tokenService = tokenService;
            _identityService = identityService;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("signup")]
        public async Task<IActionResult> Signup([FromBody]SignupModel model)
        {
            try
            {
                if(await DB.Find<Account>().Match(x => x.UserName == model.UserName).ExecuteAnyAsync())
                {
                    return BadRequest($"Account with username '{model.UserName}' already exists!");
                }

                var account = new Account(model.Name, model.UserName, model.Password);
                await account.InsertAsync();
                account = await DB.Find<Account>().MatchID(account.ID).ExecuteSingleAsync();

                return Ok(new ResultResponse<Account, string>(account, "Account Created Successfully!"));
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize(Roles = $"{AccountRole.Admin}")]
        [HttpPost]
        [Route("")]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountModel model)
        {
            try
            {
                if (await DB.Find<Account>().Match(x => x.UserName == model.UserName).ExecuteAnyAsync())
                {
                    return BadRequest($"Account with username '{model.UserName}' already exists!");
                }
                var currentAccount = await _identityService.GetCurrentAccount(HttpContext);


                var account = new Account(model.Name, model.UserName, model.Password);
                account.Role = model.Role;
                account.CreatedBy = currentAccount.ToBaseAccount();

                await account.InsertAsync();
                account = await DB.Find<Account>().MatchID(account.ID).ExecuteSingleAsync();

                return Ok(new ResultResponse<Account, string>(account, "Account Created Successfully!"));
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("")]
        public async Task<IActionResult> GetAccounts(int page = 1, int limit = 10, string? sort = "CreatedOn", bool desc = true)
        {
            try
            {
                var accounts = await DB.PagedSearch<Account>()
                    .Sort(s => desc ? s.Descending(sort) : s.Ascending(sort))
                    .PageNumber(page)
                    .PageSize(limit < 0 ? int.MaxValue : limit)
                    .ExecuteAsync();


                return Ok(new ResultResponse<IEnumerable<Account>,long>(accounts.Results, accounts.TotalCount));

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPatch]
        [Route("{id}/changepassword")]
        public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordModel model)
        {
            var account = await Authenticate(x => x.ID == id);

            if (account != null)
            {
                if (!account.Enabled)
                {
                    return BadRequest($"Account '{account.UserName}' is disabled, please contact system administrator!");
                }
                if (!account.Replace(model.OldPassword, model.NewPassword))
                {
                    return BadRequest($"Old password of Account '{account.UserName}' is incorrect!");
                }
                else
                {
                    await account.SaveAsync();

                    return Ok(new ResultResponse<Account, string>(account, "Password updated successfully!"));
                }
            }
            return NotFound("User not found!");
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var account = await Authenticate(x => x.UserName == model.UserName);

            if(account != null)
            {
                if (!VerifyPassword(account, model.Password))
                {
                    return BadRequest($"Password of Account '{account.UserName}' is incorrect!");
                }
                if (!account.Enabled)
                {
                    return BadRequest($"Account '{account.UserName}' is disabled, please contact system administrator!");
                }
                account.LastLogin = DateTime.Now;
                await account.SaveAsync();

                var token = await _tokenService.BuildToken(_config, account);
                return Ok(new ResultResponse<Account, string>(account, token));
            }
            return NotFound("User not found!");
        }


        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.User}")]
        [HttpGet]
        [Route("me")]
        public async Task<IActionResult> GetCurrentAccount()
        {
            try
            {
                var account = await _identityService.GetCurrentAccount(HttpContext);
                if(account is null)
                {
                    return NotFound("Token Invalid or Account not found.");
                }
                else
                {
                    return Ok(new ResultResponse<Account,string>(account, "Success"));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = $"{AccountRole.Admin}")]
        [HttpPatch]
        [Route("{id}")]
        public async Task<IActionResult> UpdateAccount(string id, [FromBody] AccountUpdateModel model, CancellationToken ct)
        {
            var transaction = DB.Transaction();
            try
            {
                var account = await DB.Find<Account>(transaction.Session).OneAsync(id, ct);
                if(account == null)
                {
                    return NotFound();
                }

                account.Enabled = model.Enabled ?? account.Enabled;
                account.Name = model.Name ?? account.Name;
                account.Role = model.Role ?? account.Role;
                account.ModifiedOn = DateTime.Now;

                var update = await DB.Update<Account>(transaction.Session).MatchID(account.ID).ModifyWith(account).ExecuteAsync(ct);
                await transaction.CommitAsync(ct);

                if (!update.IsAcknowledged)
                {
                    return BadRequest();
                }
                var updated = await DB.Find<Account>().OneAsync(id, ct);

                return Ok(new ResultResponse<Account, string>(updated, "Account Updated"));
            }
            catch(Exception ex)
            {
                await transaction.AbortAsync();
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = $"{AccountRole.Admin}")]
        [HttpPatch]
        [Route("")]
        public async Task<IActionResult> UpdateAccounts([FromBody] List<AccountUpdateModel> models)
        {
            var transaction = DB.Transaction();
            try
            {
                var accounts = new List<Account>();
                
                foreach (var model in models)
                {
                    var update = DB.UpdateAndGet<Account>(transaction.Session)
                        .Match(x => x.ID == model.Id);

                    if (model.Name != null) update.Modify(x => x.Name, model.Name);
                    if (model.Role != null) update.Modify(x => x.Role, model.Role);
                    if (model.Enabled != null) update.Modify(x => x.Enabled, model.Enabled);

                    var account = await update.ExecuteAsync();
                    if(account == null)
                    {
                        return NotFound($"Account with Id: {model.Id} not found");
                    }
                    accounts.Add(account);
                }
                await transaction.CommitAsync();
                return Ok(new ResultResponse<List<Account>, string>(accounts, "Accounts Updated."));
            }
            catch (Exception ex)
            {
                await transaction.AbortAsync();
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = $"{AccountRole.Admin}")]
        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteAccount(string id)
        {
            try
            {
                var delete = await DB.DeleteAsync<Account>(id);
                if (delete.IsAcknowledged)
                {
                    return Ok("Account deleted.");
                }
                else
                {
                    return NotFound();
                }
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = $"{AccountRole.Admin}")]
        [HttpDelete]
        [Route("")]
        public async Task<IActionResult> DeleteAccounts([FromBody] List<string> ids)
        {
            try
            {
                var delete = await DB.DeleteAsync<Account>(ids);
                if (!delete.IsAcknowledged)
                {
                    return BadRequest();
                }
                return Ok($"{delete.DeletedCount} Accounts Deleted.");
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        //--------------------------------------------------------------------------------------------------------------//
        private bool VerifyPassword(Account account, string password)
        {
            return account.Verify(password);
        }
        //--------------------------------------------------------------------------------------------------------------//
        private async Task<Account?> Authenticate(Expression<Func<Account, bool>> predicate)
        {
            var account = await DB.Find<Account>().Match(predicate).ExecuteFirstAsync();

            if (account != null)
            {
                return account;
            }
            return null;

        }
    }
}
