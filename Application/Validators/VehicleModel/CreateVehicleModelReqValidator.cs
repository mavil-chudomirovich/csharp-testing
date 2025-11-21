using Application.Dtos.VehicleModel.Request;
using FluentValidation;
using Application.Constants;

namespace Application.Validators.VehicleModel
{
    public class CreateVehicleModelReqValidator : AbstractValidator<CreateVehicleModelReq>
    {
        public CreateVehicleModelReqValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(Message.VehicleModelMessage.NameIsRequire);

            RuleFor(x => x.CostPerDay)
                .GreaterThan(0).WithMessage(Message.VehicleModelMessage.CostDayIdsRequired);

            RuleFor(x => x.DepositFee)
                .GreaterThanOrEqualTo(0).WithMessage(Message.VehicleModelMessage.DepositFeeIsRequired);

            RuleFor(x => x.ReservationFee)
                .GreaterThanOrEqualTo(0).WithMessage(Message.VehicleModelMessage.ReservationFeeIsRequired);

            RuleFor(x => x.SeatingCapacity)
                .GreaterThan(0).WithMessage(Message.VehicleModelMessage.SeatingCapacityIsRequired);

            RuleFor(x => x.NumberOfAirbags)
                .GreaterThanOrEqualTo(0).WithMessage(Message.VehicleModelMessage.NumberOfAirbagIsRequire);

            RuleFor(x => x.MotorPower)
                .GreaterThan(0).WithMessage(Message.VehicleModelMessage.MotorPowerIsRequired);

            RuleFor(x => x.BatteryCapacity)
                .GreaterThan(0).WithMessage(Message.VehicleModelMessage.BatteryCapacityIsRequired);

            RuleFor(x => x.EcoRangeKm)
                .GreaterThan(0).WithMessage(Message.VehicleModelMessage.EcoRangeKmIsRequired);

            RuleFor(x => x.SportRangeKm)
                .GreaterThan(0).WithMessage(Message.VehicleModelMessage.SportRangeKmIsRequired);

            RuleFor(x => x.BrandId)
                .NotEqual(Guid.Empty).WithMessage(Message.VehicleModelMessage.BrandIdIsRequired);

            RuleFor(x => x.SegmentId)
                .NotEqual(Guid.Empty).WithMessage(Message.VehicleModelMessage.SegmentIdIsRequired);
        }
    }
}