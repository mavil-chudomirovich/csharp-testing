using Application.Constants;
using Application.Dtos.Vehicle.Request;
using FluentValidation;
using System;

namespace Application.Validators.Vehicle
{
    public class CreateVehicleReqValidator : AbstractValidator<CreateVehicleReq>
    {
        public CreateVehicleReqValidator()
        {
            RuleFor(x => x.LicensePlate)
                .NotEmpty().WithMessage(Message.VehicleMessage.LicensePlateRequired)
                .Matches(@"^(0[1-9]|[1-9][0-9])[A-Z]-\d{3}\.\d{2}$")
                .WithMessage(Message.VehicleMessage.InvalidLicensePlateFormat);

            RuleFor(x => x.ModelId)
                .NotEqual(Guid.Empty).WithMessage(Message.VehicleMessage.ModelIdRequired);

            RuleFor(x => x.StationId)
                .NotEqual(Guid.Empty).WithMessage(Message.VehicleMessage.StationIdRequired);
        }
    }
}
