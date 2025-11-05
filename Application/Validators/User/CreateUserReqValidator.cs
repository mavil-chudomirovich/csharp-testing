using Application.Constants;
using Application.Dtos.User.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Application.Validators.User
{
    public class CreateUserReqValidator : AbstractValidator<CreateUserReq>
    {
        public CreateUserReqValidator()
        {
            RuleFor(x => x.DateOfBirth)
            .NotEmpty().WithMessage(Message.UserMessage.DateOfBirthIsRequired)
            .Must((req, dob) =>
            {  
                    var today = DateTime.Now;
                    var age = today.Year - dob.Year;
                    if (today.Month < dob.Month ||
                    (today.Month == dob.Month && today.Day < dob.Day))
                    {
                        age--;
                    }
                        return age >= (req.RoleName == RoleName.Customer ? 21 : 18);
                }).WithMessage(Message.UserMessage.InvalidUserAge);

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage(Message.UserMessage.PhoneIsRequired)
                .Matches(@"^(?:\+84|0)(?:3\d|5[6-9]|7\d|8[1-9]|9\d)\d{7}$")
                .WithMessage(Message.UserMessage.InvalidPhone);
        }
    }
}