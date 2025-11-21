using Application.Constants;
using Application.Dtos.Staff.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.Staff
{
    public class CreateStaffReqValidator : AbstractValidator<CreateStaffReq>
    {
        public CreateStaffReqValidator()
        {
            RuleFor(x => x.UserId)
                .NotEqual(Guid.Empty)
                .WithMessage(Message.UserMessage.UserIdIsRequired);

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(Message.UserMessage.EmailIsRequired)
                .EmailAddress().WithMessage(Message.UserMessage.InvalidEmail);

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage(Message.UserMessage.PhoneIsRequired)
                .Matches(@"^(0|\+84)([3|5|7|8|9])+([0-9]{8})$")
                .WithMessage(Message.UserMessage.InvalidPhone);

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage(Message.UserMessage.FirstNameIsRequired);

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage(Message.UserMessage.LastNameIsRequired);

            RuleFor(x => x.Sex)
                .InclusiveBetween(0, 1)
                .WithMessage(Message.UserMessage.SexIsRequired);

            RuleFor(x => x.DateOfBirth)
                .LessThan(_ => DateTimeOffset.UtcNow.AddYears(-18))
                .WithMessage(Message.UserMessage.InvalidUserAge);

            RuleFor(x => x.StationId)
                .NotEqual(Guid.Empty)
                .WithMessage(Message.UserMessage.StationIdIsRequired);

            //// Optional: validate role name if needed
            //RuleFor(x => x.RoleName)
            //    .MaximumLength(50)
            //    .WithMessage("user.role_name_too_long")
            //    .When(x => !string.IsNullOrWhiteSpace(x.RoleName));
        }
    }
}
