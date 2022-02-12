using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace UATL.MailSystem.Models.Models.Request
{
    public class DraftRequest
    {
        public string Subject { get; set; }
        public string Body { get; set; }
    }
    public class SendDraftRequest
    {
        public HashSet<string> Recipients { get; set; } = new HashSet<string>();

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public List<MailTag> Tags { get; set; } = new List<MailTag>();
       
    }
}
