using System;
using System.Collections.Generic;
using System.Text;
using NtpClient;

namespace System
{
    public static class GoogleTimeExtension
    {
        public static DateTime FromNtp(this DateTime currentTime)
        {
            try
            {
                var client = new NtpConnection("time1.google.com");
                return client.GetUtc().AddHours(1);
            }
            catch
            {
                return DateTime.Now;
            }
        }
    }
}
