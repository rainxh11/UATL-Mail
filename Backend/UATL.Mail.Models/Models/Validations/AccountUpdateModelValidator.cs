using UATL.MailSystem.Models.Request;
using FluentValidation;
using MongoDB.Bson;

namespace UATL.MailSystem.Models.Validations
{
    public class AccountUpdateModelValidator : AbstractValidator<AccountUpdateModel>
    {
        public AccountUpdateModelValidator()
        {
            /*RuleFor(x => x.Id)
                .Must(x => ObjectId.TryParse(x, out _)).WithMessage("Id must be a valid Bson ObjectID");*/

            RuleFor(x => x.Name)
                .MinimumLength(3).WithMessage("Account Name Minimum lenght is 3 characters!")
                .Matches(@"^[a-zA-Z\s]*$").WithMessage("Special characters & digits are not allowed in Account Name!");

            RuleFor(x => x.Role)
                .IsInEnum().WithMessage("Role must a valid Account Role.");

        }
    }
}
