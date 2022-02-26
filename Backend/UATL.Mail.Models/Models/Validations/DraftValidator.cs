using FluentValidation;
using MongoDB.Bson;
using System.Linq;
using UATL.MailSystem.Models.Models.Request;

namespace UATL.MailSystem.Models.Validations
{
    public class DraftValidator : AbstractValidator<DraftRequest>
    {
        public DraftValidator()
        {
            RuleFor(x => x.Subject)
                .NotEmpty().WithMessage("Draft Subject annot be empty!")
                .MinimumLength(3).WithMessage("Draft Subject Minimum length is 3 characters!")
                .MaximumLength(100).WithMessage("Draft Subject is too longe, maximum is 100 characters!");
        }
    }
    public class SendDraftValidator : AbstractValidator<SendDraftRequest>
    {
        public SendDraftValidator()
        {
            RuleFor(x => x.Recipients)
                .NotEmpty().WithMessage("Specifiy at least one Recipient!");
        }
    }

    public class SendMailRequestValidator : AbstractValidator<SendMailRequest>
    {
        public SendMailRequestValidator()
        {
            RuleFor(x => x).Custom((x, validationContext) =>
            {
                if (x.Recipients.Count == 0 && x.Type == Models.MailType.Internal)
                    validationContext.AddFailure(nameof(x.Recipients), "Recipients cannot be empty if mail is internal.");
                if (x.Recipients.Any(r => !ObjectId.TryParse(r, out _)))
                    validationContext.AddFailure(nameof(x.Recipients), "Recipients contain invalid IDs");
            });

            RuleFor(x => x.Subject)
                .NotEmpty().WithMessage("Draft Subject annot be empty!")
                .MinimumLength(3).WithMessage("Draft Subject Minimum length is 3 characters!")
                .MaximumLength(100).WithMessage("Draft Subject is too longe, maximum is 100 characters!");
        } 
    }
}
