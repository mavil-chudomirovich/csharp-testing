using Application.Repositories;
using Application.UnitOfWorks;
using Infrastructure.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.UnitOfWorks
{
    public class InvoiceUow : UnitOfwork, IInvoiceUow
    {
        public IMomoPaymentLinkRepository MomoPaymentLinkRepository { get ; set ; }
        public IInvoiceRepository InvoiceRepository { get ; set ; }
        public IInvoiceItemRepository InvoiceItemRepository { get; set; }
        public IRentalContractRepository RentalContractRepository { get; set; }
        public IDepositRepository DepositRepository { get; set; }
        public IVehicleRepository VehicleRepository { get; set; }

        public InvoiceUow(IGreenWheelDbContext context, 
            IMomoPaymentLinkRepository momoPaymentLink,
            IInvoiceRepository invoiceRepository,
            IRentalContractRepository rentalContractRepository,
            IInvoiceItemRepository invoiceItemRepository,
            IDepositRepository depositRepository, IVehicleRepository vehicleRepository) : base(context)
            {
                MomoPaymentLinkRepository = momoPaymentLink;
                InvoiceRepository = invoiceRepository;
                RentalContractRepository = rentalContractRepository;
                InvoiceItemRepository = invoiceItemRepository;
                DepositRepository = depositRepository;
                VehicleRepository = vehicleRepository;
        }
    }
}

