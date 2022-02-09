using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace UATL.Mail.Models.Request
{
    public class JsonProductModel
    {
        [JsonInclude]
        public string ProductName { get; set; }
        [JsonInclude]
        public string Barcode { get; set; }
    }
}
