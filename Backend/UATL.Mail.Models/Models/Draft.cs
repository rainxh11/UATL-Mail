using MongoDB.Driver;
using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using BsonRequired = MongoDB.Bson.Serialization.Attributes.BsonRequiredAttribute;
using BsonIgnore = MongoDB.Bson.Serialization.Attributes.BsonIgnoreAttribute;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
        public List<Attachement> Attachements { get; set; } = new List<Attachement>();
        
    }
    public enum MailTag
    {
        ResponseRequired,
        Viewed,
        TimeSensitive,
        Acknowledged,
        Archived
    }
    public class Mail : Draft
    {
        [IgnoreDefault]
        [AsObjectId]
        public string GroupId { get; set; } = null;
        [IgnoreDefault]
        [AsObjectId]
        public string ResponseTo { get; set; }

        [IgnoreDefault]
        public DateTime SentOn { get; set; }
        public AccountBase To { get; set; }
        bool IsEncrypted { get; set; } = false;

        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        [IgnoreDefault]
        public List<MailTag> Tags { get; set; } = new List<MailTag>();
    }

    public class Attachement : FileEntity, ICreatedOn, IModifiedOn
    {
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public AccountBase UploadedBy { get; set; }
        public bool Compare(string md5, long fileSize)
        {
            return MD5 == md5 && FileSize == fileSize;
        }
        bool IsEncrypted { get; set; } = false;

    }
}
