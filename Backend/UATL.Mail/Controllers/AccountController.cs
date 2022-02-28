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
using MongoDB.Driver;
using UATL.MailSystem.Models.Models;
using UATL.Mail.Helpers;
using UATL.Mail.Models.Models.Response;
using Microsoft.AspNetCore.SignalR;
using System.Linq;
using FluentEmail.Core;
using UATL.Mail.Hubs;
using Hangfire;
using UATL.Mail.Services;

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
        private readonly IHubContext<MailHub> _mailHub;
        private IBackgroundJobClient _backgroundJobs;
        private NotificationService _notificationSerivce;

        public AccountController(
            ILogger<AccountController> logger,
            IIdentityService identityService,
            ITokenService tokenService,
            IHubContext<MailHub> mailHub,
            IBackgroundJobClient bgJobs,
            NotificationService nservice,
            IConfiguration config)
        {
            _logger = logger;
            _config = config;
            _tokenService = tokenService;
            _identityService = identityService;
            _mailHub = mailHub;
            _backgroundJobs = bgJobs;
            _notificationSerivce = nservice;
        }
        //--------------------------------------------------------------------------------------------------------------//
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

                var account = new Account(model.Name, model.UserName, model.Password, model.Description);
                await account.InsertAsync();
                account = await DB.Find<Account>().MatchID(account.ID).ExecuteSingleAsync();

                return Ok(new ResultResponse<Account, string>(account, "Account Created Successfully!"));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);

                return BadRequest(ex.Message);
            }
        }

        //--------------------------------------------------------------------------------------------------------------//
        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.User}")]
        [HttpPost]
        [Route("{id}/avatar")]
        public async Task<IActionResult> AddAvatar(string id, [FromForm] IFormFile file, CancellationToken ct)
        {
            var transaction = DB.Transaction();
            try
            {
                if(!file.ContentType.Contains("image"))
                    return BadRequest(new MessageResponse<string>($"Content of type: '{file.ContentType}' not allowed! Only image type is allowed!"));

                var account = await _identityService.GetCurrentAccount(HttpContext);

                if (account.ID != id && account.Role != AccountType.Admin)
                    return Unauthorized(new MessageResponse<string>($"Only the owner of the account or an Admin account can modify Avatar!"));

                if (account.ID != id)
                    account = await DB.Find<Account>(transaction.Session).OneAsync(id, ct);

                if (account == null)
                    return NotFound();

                if (account.Avatar != null)
                await account.Avatar.DeleteAsync(transaction.Session, ct);
                var avatar = new Avatar(account);

                await avatar.SaveAsync(transaction.Session, ct);
                using (var stream = await ImageHelper.EncodeWebp(file, ct))
                {                    
                    await avatar.Data.UploadAsync(stream, cancellation: ct, session: transaction.Session);
                }
                var uploaded = await DB.Find<Avatar>(transaction.Session).OneAsync(avatar.ID);
                account.Avatar = uploaded;
                await account.SaveAsync(transaction.Session, ct);
                await transaction.CommitAsync(ct);

                return Ok(new ResultResponse<Account, string>(account, $"Avatar updated!"));
            }
            catch(Exception ex)
            {
                if (transaction.Session.IsInTransaction)
                    await transaction.AbortAsync();
                _logger.LogError(ex.Message);

                return BadRequest(ex.Message);
            }
        }
        //--------------------------------------------------------------------------------------------------------------//
        //[Authorize(Roles = $"{AccountRole.Admin},{AccountRole.User}")]
        [HttpGet]
        [Route("{id}/avatar")]
        public async Task<IActionResult> GetAvatar(string id, CancellationToken ct)
        {
            try
            {
                var avatar = await DB.Find<Avatar>().Match(x => x.Account.ID == id).ExecuteFirstAsync();
                if (avatar == null)
                    return File(new byte[] { }, "image/webp");

                var stream = new MemoryStream();               
                await avatar.Data.DownloadAsync(stream, cancellation: ct).ConfigureAwait(false);
                stream.Position = 0;
                return File(stream, "image/webp");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest();
            }
        }
        //--------------------------------------------------------------------------------------------------------------//
        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.User}")]
        [HttpDelete]
        [Route("{id}/avatar")]
        public async Task<IActionResult> DeleteAvatar(string id, CancellationToken ct)
        {
            var transaction = DB.Transaction();
            try
            {
                var account = await DB.Find<Account>(transaction.Session).OneAsync(id, ct);
                if (account == null || account.Avatar == null)
                    return NotFound();

                account.Avatar = null;
                await account.SaveAsync(transaction.Session, ct);
                await transaction.CommitAsync(ct);
                var result = await DB.DeleteAsync<Avatar>(x => x.Account.ID == id, transaction.Session, ct);

                if (result.IsAcknowledged)
                    return Ok($"Avatar Deleted.");
                else
                    return BadRequest();

            }
            catch (Exception ex)
            {
                await transaction.AbortAsync();
                _logger.LogError(ex.Message);
                return BadRequest();
            }
        }
        //--------------------------------------------------------------------------------------------------------------//
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


                var account = new Account(model.Name, model.UserName, model.Password, model.Description);
                account.Role = model.Role;
                account.CreatedBy = currentAccount.ToBaseAccount();

                await account.InsertAsync();
                account = await DB.Find<Account>().MatchID(account.ID).ExecuteSingleAsync();

                return Ok(new ResultResponse<Account, string>(account, "Account Created Successfully!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);

                return BadRequest(ex.Message);
            }
        }
        //--------------------------------------------------------------------------------------------------------------//
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
                _logger.LogError(ex.Message);

                return BadRequest(ex.Message);
            }
        }
        //--------------------------------------------------------------------------------------------------------------//
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
        //--------------------------------------------------------------------------------------------------------------//
        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model, CancellationToken ct)
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
                //_backgroundJobs.Enqueue(() => _notificationSerivce.SendEmail("rainxh11@gmail.com", "UATL MAIL Test",$"Account {account.Name} Logged in.", ct));

                return Ok(new ResultResponse<Account, string>(account, token));
            }
            return NotFound("User not found!");
        }
        //--------------------------------------------------------------------------------------------------------------//

        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.User}")]
        [HttpGet]
        [Route("me")]
        public async Task<IActionResult> GetCurrentAccount()
        {
            try
            {
                await _mailHub.Clients.All.SendAsync("refresh", "fucker");
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
                _logger.LogError(ex.Message);

                return BadRequest(ex.Message);
            }
        }
        //--------------------------------------------------------------------------------------------------------------//
        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.User}")]
        [HttpGet]
        [Route("me/avatar")]
        public async Task<IActionResult> GetCurrentAvatar(CancellationToken ct)
        {
            try
            {
                var account = await _identityService.GetCurrentAccount(HttpContext);
                if (account is null)
                    return NotFound("Token Invalid or Account not found.");

                var avatar = await DB.Find<Avatar>().Match(x => x.Account.ID == account.ID).ExecuteFirstAsync();
                if (avatar == null)
                    return NotFound();

                var stream = new MemoryStream();
                await avatar.Data.DownloadAsync(stream, cancellation: ct).ConfigureAwait(false);
                stream.Position = 0;
                return File(stream, "image/webp");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest();
            }
        }

        [HttpGet]
        [Route("me/avatar/{token}")]
        public async Task<IActionResult> GetCurrentAvatarQueryToken(string token, CancellationToken ct)
        {
            try
            {
                var account = await _identityService.GetAccountFromToken(token);
                if (account is null)
                    return NotFound("Token Invalid or Account not found.");

                var avatar = await DB.Find<Avatar>().Match(x => x.Account.ID == account.ID).ExecuteFirstAsync();
                if (avatar == null)
                    return NotFound();

                using (var stream = new MemoryStream())
                {
                    await avatar.Data.DownloadAsync(stream, cancellation: ct).ConfigureAwait(false);
                    return File(stream.ToArray(), "image/webp");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest();
            }
        }
        //--------------------------------------------------------------------------------------------------------------//
        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.User}")]
        [HttpDelete]
        [Route("me/avatar")]
        public async Task<IActionResult> DeleteCurrentAvatar(CancellationToken ct)
        {
            var transaction = DB.Transaction();
            try
            {
                var account = await _identityService.GetCurrentAccount(HttpContext);
                if (account is null)
                    return NotFound("Token Invalid or Account not found.");
                
                if (account == null || account.Avatar == null)
                    return NotFound();

                account.Avatar = null;
                await account.SaveAsync(transaction.Session, ct);
                await transaction.CommitAsync(ct);
                var result = await DB.DeleteAsync<Avatar>(x => x.Account.ID == account.ID, transaction.Session, ct);

                if (result.IsAcknowledged)
                    return Ok($"Avatar Deleted.");
                else
                    return BadRequest();

            }
            catch (Exception ex)
            {
                if (transaction.Session.IsInTransaction)
                    await transaction.AbortAsync();
                _logger.LogError(ex.Message);
                return BadRequest();
            }
        }
        //--------------------------------------------------------------------------------------------------------------//
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
                if (transaction.Session.IsInTransaction)
                    await transaction.AbortAsync();
                _logger.LogError(ex.Message);

                return BadRequest(ex.Message);
            }
        }
        //--------------------------------------------------------------------------------------------------------------//
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
                if (transaction.Session.IsInTransaction)
                    await transaction.AbortAsync();
                _logger.LogError(ex.Message);

                return BadRequest(ex.Message);
            }
        }
        //--------------------------------------------------------------------------------------------------------------//
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
        //--------------------------------------------------------------------------------------------------------------//
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
                _logger.LogError(ex.Message);

                return BadRequest(ex.Message);
            }
        }

        //--------------------------------------------------------------------------------------------------------------//
        [Authorize(Roles = $"{AccountRole.Admin},{AccountRole.User}")]
        [HttpGet]
        [Route("recipients")]
        public async Task<IActionResult> GetRecipients(string? search = "")
        {
            try
            {
                var account = await _identityService.GetCurrentAccount(HttpContext);
                var query = new List<Account>();
                search = search == "null" ? "" : search;

                if (string.IsNullOrEmpty(search))
                {
                    query = await DB.Find<Account>()
                        .Match(x => x.ID != account.ID)
                        .ExecuteAsync();
                }
                else
                {
                    query = await DB.Find<Account>()
                        .Match(x => x.ID != account.ID)
                        .ManyAsync(f => f.Regex(x => x.Name, new BsonRegularExpression($"/{search}/i")) |
                            f.Regex(x => x.UserName, new BsonRegularExpression($"/{search}/i")) |
                            f.Regex(x => x.ID, new BsonRegularExpression($"/{search}/i")) |
                            f.Regex(x => x.Description, new BsonRegularExpression($"/{search}/i"))
                        );
                }

                var recipients = query.Select(x => new Recipient(x));

                return Ok(new ResultResponse<IEnumerable<Recipient>, int>(recipients, recipients.Count()));
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
                return BadRequest();
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
