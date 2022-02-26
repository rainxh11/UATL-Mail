using MongoDB.Entities;
using System;
using System.Collections.Generic;
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
        public AccountBase ReplyTo { get; set; }

        [IgnoreDefault]
        public DateTime SentOn { get; set; }
        public AccountBase To { get; set; }
        bool IsEncrypted { get; set; } = false;

        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        [IgnoreDefault]
        public List<MailTag> Tags { get; set; } = new List<MailTag>();

        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public MailType Type { get; set; } = MailType.Internal;

        [IgnoreDefault]
        public DateTime ViewedOn { get; set; }
        public bool Viewed { get; set; } = false;
        public void SetViewed()
        {
            this.Viewed = true;
            this.ViewedOn = DateTime.Now;
        }

    }
}
