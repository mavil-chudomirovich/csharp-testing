using Application.Constants;
using Application.Dtos.Common.Request;
using Application.Dtos.Dispatch.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.Dispatch
{
    public class VehicleDispatchReqValidation : AbstractValidator<VehicleDispatchReq>
    {
        public VehicleDispatchReqValidation() 
        {
            RuleFor(x => x.ModelId)
                .NotEmpty().NotNull().WithMessage(Message.DispatchMessage.ModelRequied);
            RuleFor(x => x.NumberOfVehicle)
                .NotEmpty()
                .GreaterThan(0)
                .WithMessage(Message.DispatchMessage.NumberOfVehicleShouldGreaterThanZero);
        }
    }
}
