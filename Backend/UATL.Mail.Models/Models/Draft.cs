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
        public string Subject { get; set; }
        public string Body { get; set; } = string.Empty;
        public List<Attachment> Attachements { get; set; } = new List<Attachment>();
        public bool Starred { get; set; } = false;

    }
}
