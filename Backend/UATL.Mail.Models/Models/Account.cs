﻿using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using MongoDB.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Mapster;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace UATL.MailSystem.Models
{

    public class AccountBase : Entity
    {
        [BsonRequired]
        public string Name { get; set; }

        [BsonRequired]
        public string UserName { get; set; }
    }
    public class Account :  AccountBase, ICreatedOn, IModifiedOn
    {
        [BsonRequired]
        [JsonIgnore]
        public string PasswordHash { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime ModifiedOn { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime PasswordUpdatedOn { get; set; }
        [IgnoreDefault]
        public AccountBase CreatedBy { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AccountType Role { get; set; } = AccountType.User;
        public bool Enabled { get; set; } = true;

        public AccountBase ToBaseAccount()
        {
            return this.Adapt<AccountBase>();
        }
        public Account()
        {

        }
        public Account(string name, string userName, string password)
        {
            PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(password);
            PasswordUpdatedOn = DateTime.Now;
            Name = name;
            UserName = userName;
        }
        public static string SetPassword(string password)
        {
            return BCrypt.Net.BCrypt.EnhancedHashPassword(password);
        }
        public void CreatePassword(string password)
        {
            PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(password);
            PasswordUpdatedOn = DateTime.Now;
        }

        public static bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
        }
        public bool Verify(string password)
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(password, PasswordHash);
        }
        public bool Replace(string password, string newPassword)
        {
            if (BCrypt.Net.BCrypt.EnhancedVerify(password, PasswordHash))
            {
                PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(newPassword);
                PasswordUpdatedOn = DateTime.Now;
                ModifiedOn = DateTime.Now;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}