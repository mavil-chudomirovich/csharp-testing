using Application.Constants;
using Application.Dtos.Dispatch.Request;
using FluentValidation;
using static Application.Constants.Message;

namespace Application.Validators.Dispatch
{
    public class ConfirmDispatchReqValidator : AbstractValidator<ConfirmDispatchReq>
    {
        public ConfirmDispatchReqValidator()
        {
            RuleFor(x => x.FromStationId)
                .NotEmpty().WithMessage(DispatchMessage.FromStationIsRequire)
                .When(x => x.Status == (int)DispatchRequestStatus.Approved);

            RuleFor(x => x.FinalDescription)
                .NotEmpty().WithMessage(DispatchMessage.FinalDescriptionIsRequire)
                .When(x => x.Status == (int)DispatchRequestStatus.Approved);
        }
    }
}