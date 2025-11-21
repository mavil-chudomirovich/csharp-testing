using FluentValidation;
using Application.Constants;
using Application.Dtos.VehicleChecklist.Request;
using Application.Dtos.VehicleChecklistItem.Request;
using Application.Validators.VehicelChecklistItem;

namespace Application.Validators.VehicleChecklist
{
    public class UpdateVehicleChecklistReqValidator : AbstractValidator<UpdateVehicleChecklistReq>
    {
        public UpdateVehicleChecklistReqValidator()
        {
            RuleFor(x => x)
                .Must(x => x.IsSignedByStaff || x.IsSignedByCustomer)
                .WithMessage(Message.VehicleChecklistMessage.AtLeastOnePartyMustSign);

            RuleForEach(x => x.ChecklistItems)
                .SetValidator(new UpdateChecklistItemReqValidator())
                .When(x => x.ChecklistItems != null && x.ChecklistItems.Any());
        }
    }
}
