using Application.Constants;
using Application.Dtos.VehicleModel.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.VehicleModel
{
    public class UpdateVehicleModelReqValidator : AbstractValidator<UpdateVehicleModelReq>
    {
        public UpdateVehicleModelReqValidator()
        {

            //RuleFor(x => x.CostPerDay)
            //    .GreaterThan(0).WithMessage(Message.VehicleModelMessage.CosPerDayMustBeGreaterThanZero)
            //    .When(x => x.CostPerDay.HasValue);

            //RuleFor(x => x.DepositFee)
            //    .GreaterThanOrEqualTo(0).WithMessage(Message.VehicleModelMessage.DepositFeeMustBeGreaterThanZero)
            //    .When(x => x.DepositFee.HasValue);

            //RuleFor(x => x.ReservationFee)
            //    .GreaterThanOrEqualTo(0).WithMessage(Message.VehicleModelMessage.ReservationFeeMustBeGreaterThanZero)
            //    .When(x => x.ReservationFee.HasValue);

            //RuleFor(x => x.ReservationFee)
            //    .LessThan(x => x.CostPerDay)
            //    .WithMessage(Message.VehicleModelMessage.ReservationFeeMustBeLessThanCostPerDay)
            //    .When(x => x.ReservationFee.HasValue && x.CostPerDay.HasValue);

            //RuleFor(x => x.SeatingCapacity)
            //    .GreaterThan(0).WithMessage(Message.VehicleModelMessage.SeatingCapacityMustBeGreaterThanZero)
            //    .When(x => x.SeatingCapacity.HasValue);

            //RuleFor(x => x.NumberOfAirbags)
            //    .GreaterThanOrEqualTo(0).WithMessage(Message.VehicleModelMessage.NumberOfAirbagsMustBeGreaterThanZero)
            //    .When(x => x.NumberOfAirbags.HasValue);

            //RuleFor(x => x.MotorPower)
            //    .GreaterThan(0).WithMessage(Message.VehicleModelMessage.MotorPowerMustBeGreaterThanZero)
            //    .When(x => x.MotorPower.HasValue);

            //RuleFor(x => x.BatteryCapacity)
            //    .GreaterThan(0).WithMessage(Message.VehicleModelMessage.BatteryCapacityMustBeGreaterThanZero)
            //    .When(x => x.BatteryCapacity.HasValue);

            //RuleFor(x => x.EcoRangeKm)
            //    .GreaterThan(0).WithMessage(Message.VehicleModelMessage.EcoRangeKmMustBeGreaterThanZero)
            //    .When(x => x.EcoRangeKm.HasValue);

            //RuleFor(x => x.SportRangeKm)
            //    .GreaterThan(0).WithMessage(Message.VehicleModelMessage.SportRangeKmMustBeGreaterThanZero)
            //    .When(x => x.SportRangeKm.HasValue);
        }
    }
}
