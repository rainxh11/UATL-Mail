using MongoDB.Entities;
using System;
using Newtonsoft.Json;

namespace UATL.MailSystem.Models.Models
{
    public class Avatar : FileEntity, ICreatedOn, IModifiedOn
    {
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public Avatar(Account account)
        {
            Account = account.ToBaseAccount();
        }
        public Avatar()
        {

        }
        public string ContentType { get; set; } = "image/webp";
        [JsonIgnore]
        public AccountBase Account { get; private set; }
    }
}
