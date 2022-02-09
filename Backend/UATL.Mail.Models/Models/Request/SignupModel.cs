using System;
using System.Collections.Generic;
using System.Text;
using Mapster;

namespace UATL.Mail.Models.Request
{
    public class SignupModel
    {
        public string Name { get; set; }
        public string UserName { get; set;}
        public string Password { get; set;}
        public string ConfirmPassword { get; set; }

    }
}
