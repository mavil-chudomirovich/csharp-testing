using Application.Constants;
using Application.Dtos.InvoiceItem.Request;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Validators.InvoiceItem
{
    public class CreateInvoiceItemReqValidator : AbstractValidator<CreateInvoiceItemReq>
    {
        public CreateInvoiceItemReqValidator()
        {
            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage(Message.InvoiceMessage.InvalidUnitPrice);

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage(Message.InvoiceMessage.InvalidQuantity);

            RuleFor(x => x.Type)
                .InclusiveBetween(0, 6).WithMessage(Message.InvoiceMessage.InvoiceItemInvalidType);

        }
    }
}
