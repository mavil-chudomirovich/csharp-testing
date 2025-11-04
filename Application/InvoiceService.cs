using Application.Abstractions;
using Application.AppExceptions;
using Application.AppSettingConfigurations;
using Application.Constants;
using Application.Dtos.Common.Request;
using Application.Dtos.Common.Response;
using Application.Dtos.Invoice.Request;
using Application.Dtos.Invoice.Response;
using Application.Dtos.Momo.Request;
using Application.Helpers;
using Application.UnitOfWorks;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Application
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceUow _uow;
        private readonly IMapper _mapper;
        private readonly IMomoService _momoService;
        private readonly IEmailSerivce _emailService;
        private readonly IPhotoService _photoService;
        private readonly IMediaUow _mediaUow;
        private readonly IRentalContractService _rentalContractService;

        public InvoiceService(
            IInvoiceUow uow, 
            IMapper mapper, 
            IMomoService momoService,
            IOptions<EmailSettings> emailSettings, 
            IEmailSerivce emailSerivce, 
            IPhotoService photoService,
            IMediaUow mediaUow,
            IRentalContractService rentalContractService)
        {
            _uow = uow;
            _mapper = mapper;
            _momoService = momoService;
            _emailService = emailSerivce;
            _photoService = photoService;
            _mediaUow = mediaUow;
            _rentalContractService = rentalContractService;
        }

        public async Task PayHandoverInvoiceManual(Invoice invoice, decimal amount)
        {
            await _uow.BeginTransactionAsync();
            try
            {
                var contract = await _uow.RentalContractRepository.GetByIdAsync(invoice.ContractId)
                ?? throw new NotFoundException(Message.RentalContractMessage.NotFound);
                //lấy ra số tiền cần thanh toán (trừ cho reservation invoice nếu đã thanh toán)
                var reservationInvoice = (await _uow.InvoiceRepository.GetByContractAsync(invoice.ContractId)).FirstOrDefault(i => i.Type == (int)InvoiceType.Reservation);
                var amountNeed = InvoiceHelper.CalculateTotalAmount(invoice)
                                  - (reservationInvoice != null ? reservationInvoice.Status == (int)InvoiceStatus.Paid ? reservationInvoice.Subtotal : 0 : 0);
                if (amount < amountNeed) throw new BusinessException(Message.InvoiceMessage.InvalidAmount);
                await UpdateCashInvoice(invoice, amount);
                await CancleReservationInvoice(invoice);
                contract.Status = (int)RentalContractStatus.Active;
                await _uow.RentalContractRepository.UpdateAsync(contract);
                var subject = "[GreenWheel] Successfully Payment";
                var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "PaymentSuccessTemplate.html");
                var body = System.IO.File.ReadAllText(templatePath);
                var customer = (await _uow.RentalContractRepository.GetByIdAsync(invoice.ContractId))!.Customer;
                body = body
                     .Replace("{CustomerName}", $"{customer.LastName} {customer.FirstName}")
                     .Replace("{InvoiceCode}", invoice.Id.ToString())
                     .Replace("{PaidAmount}", $"{invoice.PaidAmount?.ToString("N0")} VND")
                     .Replace("{PaymentMethod}", Enum.GetName(typeof(PaymentMethod), invoice.PaymentMethod))
                     .Replace("{InvoiceType}", Enum.GetName(typeof(InvoiceType), invoice.Type))
                     .Replace("{PaidAt}", invoice.PaidAt?.ToString("dd/MM/yyyy HH:mm"));
                await _emailService.SendEmailAsync(customer.Email!, subject, body);
                var vehicle = await _uow.VehicleRepository.GetByIdAsync((Guid)contract.VehicleId!)
                            ?? throw new NotFoundException(Message.VehicleMessage.NotFound);
                if (vehicle.Status == (int)VehicleStatus.Available)
                {
                    vehicle.Status = (int)VehicleStatus.Unavaible;
                    await _uow.VehicleRepository.UpdateAsync(vehicle);
                }
                var anotherContract = (await _uow.RentalContractRepository.GetByVehicleIdAsync(vehicle.Id))
                                                .Where(c => c.Id != contract.Id
                                                    && (c.Status == (int)RentalContractStatus.PaymentPending
                                                        || c.Status == (int)RentalContractStatus.RequestPeding)
                                                );
                if (anotherContract != null && anotherContract.Any())
                {
                    var startBuffer = contract.StartDate.AddDays(-10);
                    var endBuffer = contract.EndDate.AddDays(10);
                    foreach (var contract_ in anotherContract)
                    {
                        if (startBuffer <= contract_.EndDate && endBuffer >= contract_.StartDate)
                        {
                            await _rentalContractService.CancelContractAndSendEmail(contract_,
                             "\r\nBooking was canceled as another customer successfully paid for the same vehicle earlier.");
                        }
                    }
                }
                await _uow.SaveChangesAsync();
                await _uow.CommitAsync();
            }catch(Exception ex)
            {
                await _uow.RollbackAsync();
                throw;
            }
        }

        public async Task PayReturnInvoiceManual(Invoice invoice, decimal amount)
        {
            var amountNeed = InvoiceHelper.CalculateTotalAmount(invoice);
            if (amount < amountNeed) throw new BusinessException(Message.InvoiceMessage.InvalidAmount);
            await UpdateCashInvoice(invoice, amount);
            await _uow.SaveChangesAsync();
        }
        public async Task PayReservationInvoiceManual(Invoice invoice, decimal amount)
        {
            await _uow.BeginTransactionAsync();
            try
            {
                var contract = await _uow.RentalContractRepository.GetByIdAsync(invoice.ContractId)
                ?? throw new NotFoundException(Message.RentalContractMessage.NotFound);
                var amountNeed = InvoiceHelper.CalculateTotalAmount(invoice);
                if (amount < amountNeed) throw new BusinessException(Message.InvoiceMessage.InvalidAmount);
                await UpdateCashInvoice(invoice, amount);
                contract.Status = (int)RentalContractStatus.Active;
                await _uow.RentalContractRepository.UpdateAsync(contract);
                await _uow.SaveChangesAsync();
                await _uow.CommitAsync();
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                throw;
            }
        }
        public async Task PayRefundInvoiceManual(Invoice invoice, decimal amount)
        {
            await _uow.BeginTransactionAsync();
            try
            {
                var contract = await _uow.RentalContractRepository.GetByIdAsync(invoice.ContractId)
                   ?? throw new NotFoundException(Message.RentalContractMessage.NotFound);
                if (contract.Status != (int)RentalContractStatus.RefundPending)
                    throw new BusinessException(Message.RentalContractMessage.ContractAlreadyProcess);
                contract.Status = (int)RentalContractStatus.Completed;
                await _uow.RentalContractRepository.UpdateAsync(contract);
                var amountNeed = InvoiceHelper.CalculateTotalAmount(invoice);
                if (amountNeed > 0 && amount < amountNeed)
                    throw new BusinessException(Message.InvoiceMessage.InvalidAmount);
                await UpdateCashInvoice(invoice, amount);
               
                
                await _uow.SaveChangesAsync();
                await _uow.CommitAsync();
            }
            catch (Exception)
            {
                await _uow.RollbackAsync();
                throw;
            }
        }
        private async Task UpdateCashInvoice(Invoice invoice, decimal amount)
        {
            invoice.PaidAmount = amount;
            invoice.PaidAt = DateTimeOffset.UtcNow;
            invoice.Status = (int)InvoiceStatus.Paid;
            invoice.PaymentMethod = (int)PaymentMethod.Cash;
            await _uow.InvoiceRepository.UpdateAsync(invoice);
            await _uow.SaveChangesAsync();
        }

        public async Task UpdateAsync(Guid invoiceId, UpdateInvoiceReq req)
        {
            var invoice = await _uow.InvoiceRepository.GetByIdAsync(invoiceId)
                ?? throw new NotFoundException(Message.InvoiceMessage.NotFound);
            invoice.PaidAmount = req.Amount;
            invoice.PaidAt = DateTimeOffset.UtcNow;
            invoice.Status = (int)InvoiceStatus.Paid;
            invoice.PaymentMethod = req.PaymentMethod;
            await _uow.InvoiceRepository.UpdateAsync(invoice);
            await _uow.SaveChangesAsync();
        }


        public async Task<string> PayHandoverInvoiceOnline(Invoice invoice, string fallbackUrl)
        {
            Invoice? reservationInvoice = null;
            reservationInvoice = (await _uow.InvoiceRepository.GetByContractAsync(invoice.ContractId)).FirstOrDefault(i => i.Type == (int)InvoiceType.Reservation);
            var amount = InvoiceHelper.CalculateTotalAmount(invoice)
                              - (reservationInvoice != null ? reservationInvoice.Status == (int)InvoiceStatus.Paid ? reservationInvoice.Subtotal : 0 : 0);
            //nếu mà reservation k null thì ktra coi status của nó có thanh toán hay chưa nếu rồi thì lấy ra phí thay
            //toán còn không thì là 0 cho mọi case
            var link = await _momoService.CreatePaymentAsync(amount, invoice.Id, invoice.Notes, fallbackUrl);
            return link;
        }

        public async Task<string> PayReservationInvoiceOnline(Invoice invoice, string fallbackUrl)
        {
            var amount = InvoiceHelper.CalculateTotalAmount(invoice);
            var link = await _momoService.CreatePaymentAsync(amount, invoice.Id, invoice.Notes, fallbackUrl);
            return link;
        }

        public async Task<string> PayReturnInvoiceOnline(Invoice invoice, string fallbackUrl)
        {
            var amount = InvoiceHelper.CalculateTotalAmount(invoice);
            var link = await _momoService.CreatePaymentAsync(amount, invoice.Id, invoice.Notes, fallbackUrl);
            return link;
        }

        //public async Task<string?> PayRefundInvoiceOnline(Invoice invoice, string fallbackUrl)
        //{
        //    await _uow.BeginTransactionAsync();
        //    try
        //    {
        //        var amount = InvoiceHelper.CalculateTotalAmount(invoice);
        //        if (amount > 0)
        //        {
        //            var link = await _momoService.CreatePaymentAsync(amount, invoice.Id, invoice.Notes, fallbackUrl);
        //            return link;
        //        }
        //        else if (amount == 0)
        //        {
        //            await UpdateCashInvoice(invoice, amount);
        //            return null;
        //        }
        //        else
        //        {
        //            throw new BadRequestException(Message.InvoiceMessage.InvalidAmount);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await _uow.RollbackAsync();
        //        throw;
        //    }
        //}

        public async Task UpdateInvoiceMomoPayment(MomoIpnReq momoIpnReq, Guid invoiceId)
        {
            await _uow.BeginTransactionAsync();
            try
            {
                await _uow.MomoPaymentLinkRepository.RemovePaymentLinkAsync(invoiceId.ToString());
                if (momoIpnReq.ResultCode == (int)MomoPaymentStatus.Success)
                {
                    var invoice = await _uow.InvoiceRepository.GetByIdAsync(invoiceId);
                    if (invoice == null)
                    {
                        throw new NotFoundException(Message.InvoiceMessage.NotFound);
                    }
                    invoice.Status = (int)InvoiceStatus.Paid;
                    invoice.PaymentMethod = (int)PaymentMethod.MomoWallet;
                    invoice.PaidAmount = momoIpnReq.Amount;
                    invoice.PaidAt = DateTimeOffset.FromUnixTimeMilliseconds(momoIpnReq.ResponseTime);
                    await _uow.InvoiceRepository.UpdateAsync(invoice);

                    //thanh toán handover thì cancel reservation
                    if (invoice.Type == (int)InvoiceType.Handover)
                    {
                        await CancleReservationInvoice(invoice);
                    }
                    await _uow.SaveChangesAsync();
                    var subject = "[GreenWheel] Successfully Payment";
                    var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "PaymentSuccessTemplate.html");
                    var body = System.IO.File.ReadAllText(templatePath);
                    var customer = (await _uow.RentalContractRepository.GetByIdAsync(invoice.ContractId))!.Customer;
                    body = body
                         .Replace("{CustomerName}", $"{customer.LastName} {customer.FirstName}")
                         .Replace("{InvoiceCode}", invoice.Id.ToString())
                         .Replace("{PaidAmount}", $"{invoice.PaidAmount?.ToString("N0")} VND")
                         .Replace("{PaymentMethod}", Enum.GetName(typeof(PaymentMethod), invoice.PaymentMethod))
                         .Replace("{InvoiceType}", Enum.GetName(typeof(InvoiceType), invoice.Type))
                         .Replace("{PaidAt}", invoice.PaidAt?.ToString("dd/MM/yyyy HH:mm"));
                    await _emailService.SendEmailAsync(customer.Email!, subject, body);
                }
                await _uow.CommitAsync();
            }catch(Exception)
            {
                await _uow.RollbackAsync();
                throw;
            }
        }

        public async Task<PageResult<Invoice>?> GetAllInvoicesAsync(PaginationParams pagination)
        {
            var invoices = await _uow.InvoiceRepository.GetAllInvoicesAsync(pagination);

            if (invoices == null || !invoices.Items.Any())
                return default;

            return invoices;
        }

        public async Task<InvoiceViewRes> GetInvoiceById(Guid id, bool includeItems = false, bool includeDeposit = false)
        {
            var invoice = await _uow.InvoiceRepository.GetByIdOptionAsync(id, includeItems, includeDeposit);
            if (invoice == null)
            {
                throw new NotFoundException(Message.InvoiceMessage.NotFound);
            }
            var reservationInvoice = (await _uow.InvoiceRepository.GetByContractAsync(invoice.ContractId))
                            .Where(i => i.Type == (int)InvoiceType.Reservation).FirstOrDefault();
            var reservationFee = 0;
            if (reservationInvoice != null && reservationInvoice.Status == (int)InvoiceStatus.Paid)
            {
                reservationFee = (int)reservationInvoice.Subtotal;
            }
            var invoiceViewRes = _mapper.Map<InvoiceViewRes>(invoice, otp => otp.Items["ReservationFee"] = reservationFee);
            return invoiceViewRes;
        }

        public async Task<IEnumerable<InvoiceViewRes>?> GetByContractIdAndStatus(Guid? contractId, int? status)
        {
            var invoices = await _uow.InvoiceRepository.GetInvoiceByContractIdAndStatus(contractId, status);
            return _mapper.Map<IEnumerable<InvoiceViewRes>?>(invoices);
        }

        public async Task<Invoice> GetRawInvoiceById(Guid id, bool includeItems = false, bool includeDeposit = false)
        {
            var invoice = await _uow.InvoiceRepository.GetByIdOptionAsync(id, includeItems, includeDeposit);
            if (invoice == null)
            {
                throw new NotFoundException(Message.InvoiceMessage.NotFound);
            }
            return invoice;
        }

        private async Task CancleReservationInvoice(Invoice handoverInvoice)
        {
            var reservationInvoice = (await _uow.InvoiceRepository.GetByContractAsync(handoverInvoice.ContractId)).FirstOrDefault(i => i.Type == (int)InvoiceType.Reservation);
            if (reservationInvoice!.Status == (int)InvoiceStatus.Pending)
            {
                reservationInvoice.Status = (int)InvoiceStatus.Cancelled;
                await _uow.InvoiceRepository.UpdateAsync(reservationInvoice);
            }
        }

        public async Task CreateAsync(CreateInvoiceReq req)
        {
            await _uow.BeginTransactionAsync();
            try
            {
                var contract = await _uow.RentalContractRepository.GetByIdAsync(req.ContractId)
                            ?? throw new NotFoundException(Message.RentalContractMessage.NotFound);
                var invoiceId = Guid.NewGuid();
                var invoice = new Invoice()
                {
                    Id = invoiceId,
                    ContractId = req.ContractId,
                    Status = (int)InvoiceStatus.Pending,
                    Tax = Common.Tax.NoneVAT, //10% dạng decimal
                    Notes = $"GreenWheel – Invoice for your order {req.ContractId}",
                    Type = req.Type
                };

                IEnumerable<InvoiceItem> items = [];
                if (req.Items != null)
                {
                    foreach (var item in req.Items)
                    {
                        items = items.Append(new InvoiceItem()
                        {
                            InvoiceId = invoiceId,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice,
                            Description = item.Description,
                            Type = item.Type,
                        });
                    }
                }
                if (req.Type == (int)InvoiceType.Refund)
                {
                    //xử lí refund cọc
                    contract.Status = (int)RentalContractStatus.RefundPending;
                    var deposit = await _uow.DepositRepository.GetByContractIdAsync(req.ContractId)
                        ?? throw new NotFoundException(Message.DispatchMessage.NotFound);
                    deposit.Status = (int)DepositStatus.Refunded;
                    items = items.Append(new InvoiceItem()
                    {
                        InvoiceId = invoiceId,
                        Quantity = 1,
                        UnitPrice = deposit.Amount,
                        Type = (int)InvoiceItemType.Refund,
                    });
                    if (items == null || !items.Any())
                    {
                        invoice.Notes.Concat(". Deposit is non-refundable due to business policy violation");
                        deposit.Status = (int)DepositStatus.Forfeited;
                    }

                    await _uow.DepositRepository.UpdateAsync(deposit);
                    await _uow.RentalContractRepository.UpdateAsync(contract);
                }
                invoice.Subtotal = InvoiceHelper.CalculateSubTotalAmount(items);
                await _uow.InvoiceRepository.AddAsync(invoice);
                await _uow.InvoiceItemRepository.AddRangeAsync(items);
                await _uow.SaveChangesAsync();
                await _uow.CommitAsync();
            }
            catch (Exception)
            {
                await _uow.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateNoteAsync(Guid id, string notes)
        {
            var invoice = await _uow.InvoiceRepository.GetByIdAsync(id)
                ?? throw new NotFoundException(Message.InvoiceMessage.NotFound);
            invoice.Notes = (string)(invoice.Notes == null ? notes : invoice.Notes.Concat($".\n {notes}"));
            await _uow.InvoiceRepository.UpdateAsync(invoice);
            await _uow.SaveChangesAsync();
        }

        public async Task<string> UploadImageAsync(Guid invoiceId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException(Message.CloudinaryMessage.NotFoundObjectInFile);

            var model = await _uow.InvoiceRepository.GetByIdAsync(invoiceId)
                ?? throw new NotFoundException(Message.InvoiceMessage.NotFound);

            var oldPublicId = model.ImagePublicId;

            var uploadReq = new UploadImageReq { File = file };
            var uploaded = await _photoService.UploadPhotoAsync(uploadReq, $"invoice/{invoiceId}/main");

            await using var trx = await _mediaUow.BeginTransactionAsync();
            try
            {
                model.ImageUrl = uploaded.Url;
                model.ImagePublicId = uploaded.PublicID;
                if(model.Type == (int)InvoiceType.Refund)
                {
                    model.Status = (int)InvoiceStatus.Paid;
                    model.PaidAt = DateTimeOffset.UtcNow;
                    model.PaidAmount = InvoiceHelper.CalculateTotalAmount(model);  
                }
                await _uow.InvoiceRepository.UpdateAsync(model);
                await _uow.SaveChangesAsync();
                await trx.CommitAsync();
              
            }
            catch
            {
                await trx.RollbackAsync();
                try { await _photoService.DeletePhotoAsync(uploaded.PublicID); } catch { }
                throw;
            }

            if (!string.IsNullOrEmpty(oldPublicId))
            {
                try { await _photoService.DeletePhotoAsync(oldPublicId); } catch { }
            }

            return model.ImageUrl!;
        }

        public async Task DeleteImageAsync(Guid modelId)
        {
            await _photoService.DeletePhotoAsync(modelId.ToString());
        }
    }
}