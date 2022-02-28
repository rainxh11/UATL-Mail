using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using System.Text.Json.Serialization;

namespace UATL.MailSystem.Models.Models.Request
{
    public class SendMailRequest : SendDraftRequest
    {
        public string Subject { get; set; }
        public string Body { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public MailType Type { get; set; }

        public ISet<string> HashTags { get; set; } = new HashSet<string>();
    }
}
