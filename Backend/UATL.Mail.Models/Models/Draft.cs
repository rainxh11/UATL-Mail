using MongoDB.Driver;
using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using BsonRequired = MongoDB.Bson.Serialization.Attributes.BsonRequiredAttribute;
using BsonIgnore = MongoDB.Bson.Serialization.Attributes.BsonIgnoreAttribute;

namespace UATL.MailSystem.Models.Models
{
    public class Draft : Entity, ICreatedOn, IModifiedOn
    {
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        [BsonRequired]
        public AccountBase From { get; set; }
        [IgnoreDefault]
        public string Subject { get; set; }
        [IgnoreDefault]
        public string Body { get; set; } = string.Empty;
        [IgnoreDefault]
        public Many<Attachment> Attachments { get; set; }

        [IgnoreDefault]
        public ISet<string> HashTags { get; set; } = new HashSet<string>();

        public Draft()
        {
            this.InitOneToMany(() => Attachments);
        }
    }
}
