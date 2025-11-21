using Application.Constants;
using Application.Dtos.Payment.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.Payment
{
    public class PaymentReqValidator : AbstractValidator<PaymentReq>
    {
        public PaymentReqValidator()
        {
            RuleFor(x => x.PaymentMethod)
                .InclusiveBetween(0, 1).WithMessage(Message.PaymentMessage.InvalidPaymentMethod);

            RuleFor(x => x.FallbackUrl)
                .NotEmpty().WithMessage(Message.PaymentMessage.FallBackUrlIsRequired)
                .Must(IsValidUrl).WithMessage(Message.PaymentMessage.InvalidFallBackUrl);

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage(Message.InvoiceMessage.InvalidAmount)
                .When(x => x.Amount.HasValue);
        }

        private bool IsValidUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
                   (result.Scheme == Uri.UriSchemeHttps || result.Scheme == Uri.UriSchemeHttp);
        }
    }
}
