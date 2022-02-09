using System;
using System.Collections.Generic;
using System.Text;

namespace UATL.Mail.Models.Request
{
    public class ChangePasswordModel
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
