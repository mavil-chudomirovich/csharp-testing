using Application.Constants;
using Application.Dtos.Momo.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.MomoPayment
{
    public class CreateMomoPaymentReqValidator : AbstractValidator<CreateMomoPaymentReq>
    {
        public CreateMomoPaymentReqValidator()
        {
            RuleFor(x => x.InvoiceId)
                .NotEqual(Guid.Empty).WithMessage(Message.PaymentMessage.InvoiceIdIsRequired);

            RuleFor(x => x.FallbackUrl)
                .NotEmpty().WithMessage(Message.PaymentMessage.FallBackUrlIsRequired)
                .Must(IsValidUrl).WithMessage(Message.PaymentMessage.InvalidFallBackUrl);
        }

        private bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
                   (result.Scheme == Uri.UriSchemeHttps || result.Scheme == Uri.UriSchemeHttp);
        }
    }
}
