using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Mapster;
using MongoDB.Bson.Serialization.Attributes;

namespace UATL.MailSystem.Models.Request
{
    public class SignupModel
    {
        public string Name { get; set; }
        public string UserName { get; set;}
        public string Password { get; set;}
        public string ConfirmPassword { get; set; }
#nullable enable
        public string? Description { get; set; }
#nullable disable

    }
    public class CreateAccountModel : SignupModel
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AccountType Role { get; set; }
    }
}
