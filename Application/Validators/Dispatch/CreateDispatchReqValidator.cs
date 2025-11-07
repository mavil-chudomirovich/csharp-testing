using Application.Constants;
using Application.Dtos.Dispatch.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.Dispatch
{
    public class CreateDispatchReqValidator : AbstractValidator<CreateDispatchReq>
    {
        public CreateDispatchReqValidator()
        {
            //RuleFor(x => x.FromStationId)
            //    .NotEmpty().WithMessage(Message.DispatchMessage.FromStationIsRequire);
        }
    }
}