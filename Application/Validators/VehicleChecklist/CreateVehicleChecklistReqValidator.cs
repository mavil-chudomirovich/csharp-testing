using Application.Dtos.VehicleChecklist.Request;
using FluentValidation;
using Application.Constants;

namespace Application.Validators.VehicleChecklist
{
    public class CreateVehicleChecklistReqValidator : AbstractValidator<CreateVehicleChecklistReq>
    {
        public CreateVehicleChecklistReqValidator()
        {
            RuleFor(x => x.Type)
                .InclusiveBetween(0, 2)
                .WithMessage(Message.VehicleChecklistMessage.InvalidType);
        }
    }
}