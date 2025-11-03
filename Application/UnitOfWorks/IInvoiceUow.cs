using Application.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UnitOfWorks
{
    public interface IInvoiceUow : IUnitOfwork
    {
        IMomoPaymentLinkRepository MomoPaymentLinkRepository { get; set; }
        IInvoiceRepository InvoiceRepository { get; set; }
        IRentalContractRepository RentalContractRepository { get; set; }
        IInvoiceItemRepository InvoiceItemRepository { get; set; }
        IDepositRepository DepositRepository { get; set; }
        IVehicleRepository VehicleRepository { get; set; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    
    }
}
