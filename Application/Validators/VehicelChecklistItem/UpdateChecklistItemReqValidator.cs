using Application.Dtos.VehicleChecklistItem.Request;
using FluentValidation;
using Application.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.VehicelChecklistItem
{
    public class UpdateChecklistItemReqValidator : AbstractValidator<UpdateChecklistItemReq>
    {
        public UpdateChecklistItemReqValidator()
        {
            RuleFor(x => x.Id)
                .NotNull()
                .WithMessage(Message.VehicleChecklistItemMessage.NotFound);

            RuleFor(x => x.Status)
                .InclusiveBetween(0, 4)
                .WithMessage(Message.VehicleChecklistMessage.InvalidStatus);
        }
    }
}