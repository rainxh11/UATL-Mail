using MongoDB.Entities;
using System;
using System.IO;

namespace UATL.MailSystem.Models.Models
{
    public class Attachment : FileEntity, ICreatedOn, IModifiedOn
    {
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string Name { get; set; }
        public string Extension
        {
            get => Path.GetExtension(Name).TrimStart('.');
        }
        public string ContentType { get; set; }
        public AccountBase UploadedBy { get; set; }
        public bool Compare(string md5, long fileSize)
        {
            return MD5 == md5 && FileSize == fileSize;
        }
        bool IsEncrypted { get; set; } = false;

    }
}
