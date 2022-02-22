using System;
using System.Collections.Generic;
using System.Text;
using UATL.MailSystem.Models;

namespace UATL.Mail.Models.Models.Response
{
    public class Recipient
    {
        public Recipient(Account account)
        {
            ID = account.ID;
            UserName = account.UserName;
            Name = account.Name;
            Description = account.Description;
            Avatar = $"/account/{account.ID}/avatar";
        }
        public string ID { get; private set; }
        public string UserName { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Avatar { get; private set; }
    }
}
