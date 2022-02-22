using MongoDB.Entities;
using MongoDB.Bson.Serialization.Attributes;

namespace UATL.MailSystem.Models
{
    public class AccountBase : Entity
    {
        [BsonRequired]
        public string Name { get; set; }

        [BsonRequired]
        public string UserName { get; set; }
    }
}