using MongoDB.Entities;
using System;

namespace UATL.MailSystem.Models.Models
{
    [Collection("Attachement")]
    public class Attachment : FileEntity, ICreatedOn, IModifiedOn
    {
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string Name { get; set; }
        public string ContentType { get; set; }
        public AccountBase UploadedBy { get; set; }
        public bool Compare(string md5, long fileSize)
        {
            return MD5 == md5 && FileSize == fileSize;
        }
        bool IsEncrypted { get; set; } = false;

    }
}
