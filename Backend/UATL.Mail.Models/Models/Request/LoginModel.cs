using System;
using System.Collections.Generic;
using System.Text;

namespace UATL.Mail.Models.Request
{
    public class LoginModel
    {
        public LoginModel()
        {

        }
        public LoginModel(string username, string password)
        {
            this.UserName = username;
            this.Password = password;
        }
        public string UserName { get; set;}
        public string Password { get; set;}
    }
}
