using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using MongoDB.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Mapster;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UATL.MailSystem.Models.Models;

namespace UATL.MailSystem.Models
{
    public class Account :  AccountBase, ICreatedOn, IModifiedOn
    {
        [BsonRequired]
        [JsonIgnore]
        public string PasswordHash { get; set; }
        public DateTime CreatedOn { get; set; } = DateTime.Now;
        public DateTime ModifiedOn { get; set; }
        public DateTime LastLogin { get; set; }
        public DateTime PasswordUpdatedOn { get; set; }
        public string Description { get; set; }
        [IgnoreDefault]
        public AccountBase CreatedBy { get; set; }

        //[JsonConverter(typeof(StringEnumConverter))]
        [JsonIgnore]
        public AccountType Role { get; set; } = AccountType.User;

        [JsonProperty("Role")]
        public string RoleName { get => Role.ToString(); }
        public bool Enabled { get; set; } = true;

        [IgnoreDefault]
        public Avatar Avatar { get; set; }

        public AccountBase ToBaseAccount()
        {
            return this.Adapt<AccountBase>();
        }
        public Account()
        {
            this.InitOneToMany(() => Starred);
        }
        public Account(string name, string userName, string password, string description = "")
        {
            PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(password, 13);
            PasswordUpdatedOn = DateTime.Now;
            Name = name;
            UserName = userName;
            Description = description;
        }
        public static string SetPassword(string password)
        {
            return BCrypt.Net.BCrypt.EnhancedHashPassword(password, 13);
        }
        public void CreatePassword(string password)
        {
            PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(password, 13);
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
                PasswordHash = BCrypt.Net.BCrypt.EnhancedHashPassword(newPassword, 13);
                PasswordUpdatedOn = DateTime.Now;
                ModifiedOn = DateTime.Now;
                return true;
            }
            else
            {
                return false;
            }
        }
        [JsonIgnore]
        public Many<MailModel> Starred { get; set; }
    }
}