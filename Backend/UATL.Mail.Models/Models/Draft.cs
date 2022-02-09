using MongoDB.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace UATL.Mail.Models.Models
{
    public class Draft : Entity, ICreatedOn, IModifiedOn
    {
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }

        public One<Account> From { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        [OwnerSide]
        public Many<Attachement> Attachements { get; set;}
        public Draft()
        {
            this.InitManyToMany(() => Attachements, att => att.Drafts);
        }
        
    }
    public enum MailType
    {
        Sent,
        Received,
        Scheduled
    }
    public enum MailTag
    {
        ResponseRequired,
        Viewed,
        TimeSensitive,
        Acknowledged,
        Archived
    }
    public class Mail : Draft
    {
        [IgnoreDefault]
        public string GroupId { get; set; } = null;
        [IgnoreDefault]
        public One<Mail> ResponseTo { get; set; }

        public DateTime SentOn { get; set; }
        public One<Account> To { get; set; }
        public Mail()
        {
            this.InitManyToMany(() => Attachements, att => att.Mails);
        }
    }

    public class Attachement : FileEntity, ICreatedOn, IModifiedOn
    {
        public Attachement()
        {
            this.InitManyToMany(() => Drafts, draft => draft.Attachements);
            this.InitManyToMany(() => Mails, mail => mail.Attachements);
        }
        public Many<Draft> Drafts { get; set; }
        public Many<Mail> Mails { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string Name { get; set; }
        public One<Account> UploadedBy { get; set; }
    }
}
