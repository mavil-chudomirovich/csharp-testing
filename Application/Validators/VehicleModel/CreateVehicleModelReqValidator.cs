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
                .GreaterThan(0).WithMessage(Message.VehicleModelMessage.CosPerDayMustBeGreaterThanZero);

            RuleFor(x => x.DepositFee)
                .GreaterThanOrEqualTo(0).WithMessage(Message.VehicleModelMessage.DepositFeeMustBeGreaterThanZero);

            RuleFor(x => x.ReservationFee)
                .GreaterThanOrEqualTo(0).WithMessage(Message.VehicleModelMessage.ReservationFeeMustBeGreaterThanZero);

            RuleFor(x => x.ReservationFee)
                .LessThan(x => x.CostPerDay)
                .WithMessage(Message.VehicleModelMessage.ReservationFeeMustBeLessThanCostPerDay);
            RuleFor(x => x.SeatingCapacity)
                .GreaterThan(0).WithMessage(Message.VehicleModelMessage.SeatingCapacityMustBeGreaterThanZero);

            RuleFor(x => x.NumberOfAirbags)
                .GreaterThanOrEqualTo(0).WithMessage(Message.VehicleModelMessage.NumberOfAirbagsMustBeGreaterThanZero);

            RuleFor(x => x.MotorPower)
                .GreaterThan(0).WithMessage(Message.VehicleModelMessage.MotorPowerMustBeGreaterThanZero);

            RuleFor(x => x.BatteryCapacity)
                .GreaterThan(0).WithMessage(Message.VehicleModelMessage.BatteryCapacityMustBeGreaterThanZero);

            RuleFor(x => x.EcoRangeKm)
                .GreaterThan(0).WithMessage(Message.VehicleModelMessage.EcoRangeKmMustBeGreaterThanZero);

            RuleFor(x => x.SportRangeKm)
                .GreaterThan(0).WithMessage(Message.VehicleModelMessage.SportRangeKmMustBeGreaterThanZero);

            RuleFor(x => x.BrandId)
                .NotEqual(Guid.Empty).WithMessage(Message.VehicleModelMessage.BrandIdIsRequired);

            RuleFor(x => x.SegmentId)
                .NotEqual(Guid.Empty).WithMessage(Message.VehicleModelMessage.SegmentIdIsRequired);
        }
    }
}