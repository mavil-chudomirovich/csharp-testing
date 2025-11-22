using Application.Constants;
using Application.Dtos.User.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.User
{
    public class UserChangPasswordReqValidator : AbstractValidator<UserChangePasswordReq>
    {
        public UserChangPasswordReqValidator()
        {
            RuleFor(x => x.OldPassword)
               //.NotEmpty().WithMessage(Message.Register.PasswordCanNotEmpty)
               .MinimumLength(8).WithMessage(Message.UserMessage.PasswordTooShort);

            RuleFor(x => x.Password)
               .NotEmpty().WithMessage(Message.UserMessage.PasswordCanNotEmpty)
               .MinimumLength(8).WithMessage(Message.UserMessage.PasswordTooShort)
               .Matches("^(?=.*[A-Z])(?=.*[0-9])(?=.*[!@#$%^&])[A-Za-z0-9!@#$%^&*]{8,}$")
               .WithMessage(Message.UserMessage.PasswordStrength);

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage(Message.UserMessage.ConfirmPasswordIsIncorrect);
        }
    }
}
