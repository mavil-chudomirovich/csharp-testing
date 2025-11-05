using Application.Constants;
using Application.Dtos.RentalContract.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.RentalContract
{
    public class ConfirmReqValidator : AbstractValidator<ConfirmReq>
    {
        public ConfirmReqValidator()
        {
            //RuleFor(x => x.VehicleStatus)
            //    .NotNull().WithMessage(Message.RentalContractMessage.InvalidVehicleStatus)
            //    .InclusiveBetween(0, 2).WithMessage(Message.RentalContractMessage.InvalidVehicleStatus)
            //    .When(x => !x.HasVehicle);
        }
    }
}
