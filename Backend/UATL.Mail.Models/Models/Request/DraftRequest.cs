using System;
using System.Collections.Generic;
using System.Text;

namespace UATL.MailSystem.Models.Models.Request
{
    public class DraftRequest
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public ISet<string> HashTags { get; set; } = new HashSet<string>();
    }
}
