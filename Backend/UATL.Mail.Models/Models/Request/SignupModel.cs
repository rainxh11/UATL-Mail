﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using Mapster;
using MongoDB.Bson.Serialization.Attributes;

namespace UATL.Mail.Models.Request
{
    public class SignupModel
    {
        public string Name { get; set; }
        public string UserName { get; set;}
        public string Password { get; set;}
        public string ConfirmPassword { get; set; }

    }
    public class CreateAccountModel : SignupModel
    {
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AccountType Role { get; set; }
    }
}
