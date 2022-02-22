using Newtonsoft.Json.Converters;
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
        public List<string> Recipients { get; set; } = new List<string>();

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public List<MailTag> Tags { get; set; } = new List<MailTag>();
       
    }
    
    public class SendMailRequest
    {
        public List<string> Recipients { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MailType Type { get; set; }
    }
}
