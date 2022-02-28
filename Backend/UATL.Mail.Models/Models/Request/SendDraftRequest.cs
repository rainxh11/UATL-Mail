using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace UATL.MailSystem.Models.Models.Request
{
    public class SendDraftRequest
    {
        public List<string> Recipients { get; set; } = new List<string>();

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public List<MailFlag> Flags { get; set; } = new List<MailFlag>();

    }
}
