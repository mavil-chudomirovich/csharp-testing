using Application.Dtos.Dispatch.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.Constants.Message;

namespace Application.Validators.Dispatch
{
    public class UpdateDispatchReqValidator : AbstractValidator<UpdateDispatchReq>
    {
        public UpdateDispatchReqValidator()
        {
            RuleFor(x => x.Status)
                .InclusiveBetween(0, 5) 
                .WithMessage(DispatchMessage.InvalidStatus);
        }
    }
}
