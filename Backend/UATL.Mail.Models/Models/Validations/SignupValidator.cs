using UATL.Mail.Models.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UATL.Mail.Models.Models.Request;

namespace UATL.Mail.Models.Validations
{
    public class SignupValidator : AbstractValidator<SignupModel>
    {
        public SignupValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Account Name cannot be empty!")
                .MinimumLength(3).WithMessage("Account Name Minimum length is 3 characters!")
                .Matches(@"^[a-zA-Z\s]*$").WithMessage("Special characters & digits are not allowed in Account Name!");

            RuleFor(x => x.UserName)
                .NotEmpty().WithMessage("Username cannot be empty!")
                .MinimumLength(5).WithMessage("Username length is 5 characters minimum!")
                .Matches(@"^[a-zA-Z0-9]*$").WithMessage("Special characters & empty spaces are not allowed in Username!");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Password cannot be empty!")
                .MinimumLength(8).WithMessage("Password length is 8 characters minimum!")
                .MaximumLength(20).WithMessage("Password maximum length is 20 characters!");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password cannot be empty!")
                .MinimumLength(8).WithMessage("Password length is 8 characters minimum!")
                .Equal(x => x.ConfirmPassword).WithMessage("Passowrd & Confirmation are not equal!")
                .MaximumLength(20).WithMessage("Password maximum length is 20 characters!");
        }
    }
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
}
