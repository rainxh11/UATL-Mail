using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Entities;
using UATL.MailSystem.Models;
using UATL.MailSystem.Models.Models;
using UATL.MailSystem.Models.Models.Request;

namespace UATL.Mail.Helpers
{
    public class MailRequestHelper
    {
        public static async Task<List<MailModel>> GetMails(SendMailRequest? request, Account account, CancellationToken ct = default, IClientSessionHandle session = null)
        {
            var mails = new List<MailModel>();
            var groupId = ObjectId.GenerateNewId().ToString();
            var body = ModelHelper.ReplaceHref(request.Body);

            foreach (var recipient in request.Recipients.Where(x => x != account.ID))
            {
                var destinationAccount = await DB.Find<Account>(session).OneAsync(recipient, ct);
                if (destinationAccount == null)
                    throw new Exception($"Recipient with id:'{recipient}' not found!");

                var mail = new MailModel()
                {
                    ID = ObjectId.GenerateNewId().ToString(),
                    Body = body,
                    Subject = request.Subject,
                    From = account.ToBaseAccount(),
                    To = destinationAccount.ToBaseAccount(),
                    SentOn = DateTime.Now,
                    Type = request.Type,
                    Flags = request.Flags,
                    GroupId = groupId,
                    HashTags = request.HashTags
                };
                mails.Add(mail);
            }
            return mails;
        }
    }
}
