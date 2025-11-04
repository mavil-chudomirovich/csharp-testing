using Application.Dtos.Invoice.Request;
using Application.Dtos.InvoiceItem.Request;
using FluentValidation;
using System;
using System.Linq;
using static Application.Constants.Message;

namespace Application.Validators.Invoice
{
    public class CreateInvoiceReqValidator : AbstractValidator<CreateInvoiceReq>
    {
        public CreateInvoiceReqValidator()
        {
            RuleFor(x => x.ContractId)
                .NotEqual(Guid.Empty)
                .WithMessage(InvoiceMessage.NotFound);

            RuleFor(x => x.Type)
                .InclusiveBetween(0, 5)
                .WithMessage(InvoiceMessage.InvalidInvoiceType);

            RuleFor(x => x.Items)
                .NotNull()
                .WithMessage(InvoiceMessage.NotFound)
                .Must(items => items.Any())
                .WithMessage(InvoiceMessage.NotFound);
        }
    }
}
