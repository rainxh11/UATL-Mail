using MongoDB.Entities;
using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UATL.MailSystem.Models.Models
{

    [Collection("Mail")]
    public class MailModel : Draft
    {
        [IgnoreDefault]
        [AsObjectId]
        public string GroupId { get; set; } = null;
        [IgnoreDefault]
        public One<MailModel> ReplyTo { get; set; }

        [IgnoreDefault]
        public DateTime SentOn { get; set; }
        public AccountBase To { get; set; }
        bool IsEncrypted { get; set; } = false;

        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        [IgnoreDefault]
        public List<MailFlag> Flags { get; set; } = new List<MailFlag>();

        [JsonIgnore]
        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public MailType Type { get; set; } = MailType.Internal;
        [JsonProperty("Type")]
        public string TypeName
        {
            get => Type.ToString();
        }

        [IgnoreDefault]
        public DateTime ViewedOn { get; set; }
        public bool Viewed { get; set; } = false;

        public bool Approved
        {
            get
            {
                if (Type == MailType.External)
                {
                    return Flags.Contains(MailFlag.Approved);
                }
                else
                {
                    return false;
                }
            }
        }

        [IgnoreDefault] public AccountBase ApprovedBy { get; set; } = null;


        public bool Acknowledged
        {
            get => Flags.Contains(MailFlag.Acknowledged);
        }
        public bool RequireTask
        {
            get => Flags.Contains(MailFlag.RequireTask);
        }

        public bool Important
        {
            get => Flags.Contains(MailFlag.Important);
        }

        public bool Reviewed
        {
            get => Flags.Contains(MailFlag.Reviewed);
        }
        public void SetViewed()
        {
            this.Viewed = true;
            this.ViewedOn = DateTime.Now;
        }

        public MailModel()
        {
            this.InitOneToMany(() => Attachments);
        }
        [BsonIgnore]
        public IEnumerable<AccountBase>? Recipients { get; set; }
    }
}
