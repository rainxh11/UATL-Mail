using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace UATL.Mail.Models.Request
{
    public class AccountUpdateModel
    {
        public string Id { get; set; }
#nullable enable
        public string? Name { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AccountType? Role { get; set; }
        public bool? Enabled { get; set; }
#nullable disable
    }
}
