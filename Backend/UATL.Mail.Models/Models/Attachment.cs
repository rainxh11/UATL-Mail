using System;
using System.IO;
using MongoDB.Entities;

namespace UATL.MailSystem.Common.Models;

public class Attachment : FileEntity, ICreatedOn, IModifiedOn
{
    public string Name { get; set; }

    public string Extension => Path.GetExtension(Name).TrimStart('.');

    public string ContentType { get; set; }
    public AccountBase UploadedBy { get; set; }
    private bool IsEncrypted { get; set; } = false;
    public DateTime CreatedOn { get; set; }
    public DateTime ModifiedOn { get; set; }

    public bool Compare(string md5, long fileSize)
    {
        return MD5 == md5 && FileSize == fileSize;
    }
}