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
    public class VehicleFilterReqValidator : AbstractValidator<VehicleFilterReq>
    {
        public VehicleFilterReqValidator()
        {
            RuleFor(x => x.StationId)
                .NotEqual(Guid.Empty)
                .WithMessage(Message.VehicleMessage.StationIdRequired);

            RuleFor(x => x.StartDate)
                .GreaterThan(_ => DateTimeOffset.UtcNow)
                .WithMessage(Message.RentalContractMessage.StartDateMustBeFuture);

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate)
                .WithMessage(Message.VehicleModelMessage.RentTimeIsNotAvailable);
        }
    }
}
