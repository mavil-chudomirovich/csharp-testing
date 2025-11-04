using Application.Abstractions;
using Application.AppExceptions;
using Application.AppSettingConfigurations;
using Application.Constants;
using Application.Dtos.Common.Request;
using Application.Dtos.Common.Response;
using Application.Dtos.RentalContract.Request;
using Application.Dtos.RentalContract.Respone;
using Application.Helpers;
using Application.Repositories;
using Application.UnitOfWorks;
using AutoMapper;
using Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Diagnostics.Contracts;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using static System.Collections.Specialized.BitVector32;

namespace Application
{
    public class RentalContractService : IRentalContractService
    {
        private readonly IRentalContractUow _uow;
        private readonly IMapper _mapper;
        private readonly IEmailSerivce _emailService;
        private readonly IMemoryCache _cache;
        private readonly IStaffRepository _staffRepository;

        public RentalContractService(IRentalContractUow uow, IMapper mapper,
            IOptions<EmailSettings> emailSettings, IEmailSerivce emailService,
            IMemoryCache cache, IStaffRepository staffRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _emailService = emailService;
            _cache = cache;
            _staffRepository = staffRepository;
        }

        public async Task<RentalContractViewRes> GetByIdAsync(Guid id)
        {
            var contract = await _uow.RentalContractRepository.GetByIdAsync(id);
            if (contract == null) throw new NotFoundException(Message.RentalContractMessage.NotFound);
            var reservationInvoice = (await _uow.InvoiceRepository.GetByContractAsync(id))
                            .Where(i => i.Type == (int)InvoiceType.Reservation).FirstOrDefault();
            var reservationFee = 0;
            if (reservationInvoice != null && reservationInvoice.Status == (int)InvoiceStatus.Paid)
            {
                reservationFee = (int)reservationInvoice.Subtotal;
            }
            return _mapper.Map<RentalContractViewRes>(contract, otp => otp.Items["ReservationFee"] = reservationFee);
        }

        public async Task CreateRentalContractAsync(Guid userID, CreateRentalContractReq createReq)
        {
            await _uow.BeginTransactionAsync();
            try
            {
                //ktra xem có cccd hay chưa
                var citizenIdentity = await _uow.CitizenIdentityRepository.GetByUserIdAsync(userID);
                if (citizenIdentity == null)
                {
                    throw new ForbidenException(Message.UserMessage.CitizenIdentityNotFound);
                }
                var driverLisence = await _uow.DriverLicenseRepository.GetByUserIdAsync(userID);
                if (driverLisence == null)
                {
                    throw new ForbidenException(Message.UserMessage.DriverLicenseNotFound);
                }
                //---------
                //ktra có đơn đặt xe chưa
                if (await _uow.RentalContractRepository.HasActiveContractAsync(userID))
                {
                    throw new BusinessException(Message.RentalContractMessage.UserAlreadyHaveContract);
                }
                var station = await _uow.StationRepository.GetByIdAsync(createReq.StationId) ??
                    throw new NotFoundException(Message.StationMessage.NotFound);

                var model = await _uow.VehicleModelRepository.GetByIdAsync(createReq.ModelId,
                    createReq.StationId, createReq.StartDate, createReq.EndDate);

                if (model!.Vehicles == null || model.Vehicles.Count == 0) throw new NotFoundException(Message.VehicleMessage.NotFound);
                var vehicle = model.Vehicles?.FirstOrDefault();
                if (vehicle == null)
                {
                    throw new NotFoundException(Message.VehicleMessage.NotFound);
                }
                var days = (int)Math.Ceiling((createReq.EndDate - createReq.StartDate).TotalDays);

                Guid contractId = Guid.NewGuid();
                var contract = new RentalContract()
                {
                    Id = contractId,
                    Description = $"This contract was created by the customer through the online booking system." +
                    $"\r\nThe vehicle will be reserved at {station.Name} from {createReq.StartDate} to {createReq.EndDate}." +
                    $"\r\nCustomer rented the vehicle for {days} days.",
                    Notes = createReq.Notes,
                    StartDate = createReq.StartDate,
                    ActualStartDate = null,
                    EndDate = createReq.EndDate,
                    ActualEndDate = null,
                    IsSignedByCustomer = false,
                    IsSignedByStaff = false,
                    Status = (int)RentalContractStatus.RequestPeding,
                    VehicleId = vehicle.Id,
                    CustomerId = userID,
                    StationId = station.Id,
                    HandoverStaffId = null,
                    ReturnStaffId = null,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    DeletedAt = null,
                };
                await _uow.RentalContractRepository.AddAsync(contract);

                Guid handoverInvoiceId = Guid.NewGuid();
                Guid reservationInvoiceId = Guid.NewGuid();

                var businessVariables = _cache!.Get<List<BusinessVariable>>(Common.SystemCache.BusinessVariables);
                var baseVat = businessVariables!.FirstOrDefault(b => b.Key == (int)BusinessVariableKey.BaseVAT)?.Value;
                var handoverInvoice = new Invoice()
                {
                    Id = handoverInvoiceId,
                    ContractId = contractId,
                    Status = (int)InvoiceStatus.Pending,
                    Tax = (decimal)baseVat!, //10% dạng decimal
                    Type = (int)InvoiceType.Handover,
                    Notes = $"GreenWheel – Invoice for your order {contractId}"
                };
                var reservationInvoice = new Invoice()
                {
                    Id = reservationInvoiceId,
                    ContractId = contractId,
                    Status = (int)InvoiceStatus.Pending,
                    Tax = Common.Tax.NoneVAT, //10% dạng decimal
                    Type = (int)InvoiceType.Reservation,
                    Notes = $"GreenWheel – Invoice for your order {contractId}"
                };
                await _uow.InvoiceRepository.AddRangeAsync([handoverInvoice, reservationInvoice]);
                var baseRentalItem = new InvoiceItem()
                {
                    InvoiceId = handoverInvoiceId,
                    Quantity = days,
                    UnitPrice = model.CostPerDay,
                    Type = (int)InvoiceItemType.BaseRental,
                };
                var reservationRentalItem = new InvoiceItem()
                {
                    InvoiceId = reservationInvoiceId,
                    Quantity = 1,
                    UnitPrice = model.ReservationFee,
                    Type = (int)InvoiceItemType.Other,
                };

                await _uow.InvoiceItemRepository.AddRangeAsync([baseRentalItem, reservationRentalItem]);
                var deposit = new Deposit
                {
                    InvoiceId = handoverInvoiceId,
                    Amount = model.DepositFee,
                    Status = (int)DepositStatus.Pending,
                };

                await _uow.DepositRepository.AddAsync(deposit);
                handoverInvoice.Subtotal = InvoiceHelper.CalculateSubTotalAmount([baseRentalItem]);
                reservationInvoice.Subtotal = InvoiceHelper.CalculateSubTotalAmount([reservationRentalItem]);

                await _uow.SaveChangesAsync();
                await _uow.CommitAsync();
                await _uow.BeginTransactionAsync();
            }
            catch (Exception)
            {
                await _uow.RollbackAsync();
                throw;
            }
        }

        //public async Task<IEnumerable<RentalContractViewRes>> GetAll(GetAllRentalContactReq req)
        //{
        //    var contracts = await _uow.RentalContractRepository.GetAllAsync(req.Status, req.Phone,
        //        req.CitizenIdentityNumber, req.DriverLicenseNumber, req.StationId);
        //    return _mapper.Map<IEnumerable<RentalContractViewRes>>(contracts) ?? [];

        //}

        //public async Task<IEnumerable<RentalContractViewRes>> GetMyContracts(ClaimsPrincipal userClaims, int? status)
        //{
        //    var userId = userClaims.FindFirst(JwtRegisteredClaimNames.Sid).Value.ToString();
        //    var contracts = await _uow.RentalContractRepository.GetByCustomerAsync(Guid.Parse(userId), status);
        //    return _mapper.Map<IEnumerable<RentalContractViewRes>>(contracts) ?? [];
        //}

        public async Task HandoverProcessRentalContractAsync(ClaimsPrincipal userClaims, Guid id, HandoverContractReq req)
        {
            var userId = userClaims.FindFirst(JwtRegisteredClaimNames.Sid)!.Value.ToString();
            var user = await _staffRepository.GetByUserIdAsync(Guid.Parse(userId));
            if(user != null)
            {
                if(!(await VerifyStaffPermission(userClaims, id)))
                {
                    throw new ForbidenException(Message.UserMessage.DoNotHavePermission);
                }
            }
            await _uow.BeginTransactionAsync();
            try
            {
                var contract = await _uow.RentalContractRepository.GetByIdAsync(id)
                    ?? throw new NotFoundException(Message.RentalContractMessage.NotFound);
                if (contract.ActualStartDate != null) throw new BusinessException(Message.RentalContractMessage.ContractAlreadyProcess);
                if (contract.StartDate > DateTimeOffset.UtcNow)
                {
                    throw new BadRequestException(Message.RentalContractMessage.ContractNotStartYet);
                }
                var vehicle = await _uow.VehicleRepository.GetByIdAsync((Guid)contract.VehicleId!)
                    ?? throw new NotFoundException(Message.VehicleMessage.NotFound);

                var handoverInvoice = (await _uow.InvoiceRepository.GetByContractAsync(id))
                    .Where(i => i.Type == (int)InvoiceType.Handover).FirstOrDefault()
                        ?? throw new NotFoundException(Message.InvoiceMessage.NotFound);

                if (contract.VehicleChecklists == null ||
                    !contract.VehicleChecklists.Any(c => c.Type == (int)VehicleChecklistType.Handover))
                {
                    throw new NotFoundException(Message.VehicleChecklistMessage.NotFound);
                }
                if (contract.Status == (int)RentalContractStatus.Active && handoverInvoice.Status == (int)InvoiceStatus.Paid)
                {
                    if (vehicle == null)
                    {
                        throw new NotFoundException(Message.VehicleMessage.NotFound);
                    }
                    vehicle.Status = (int)VehicleStatus.Rented;
                    await _uow.VehicleRepository.UpdateAsync(vehicle);
                    //lụm xe đi chơi đi bạn
                }
                else
                {
                    throw new BusinessException(Message.InvoiceMessage.NotHandoverPayment);
                }
                if (req.IsSignedByStaff && contract.IsSignedByStaff == false)
                {
                    contract.IsSignedByStaff = req.IsSignedByStaff;
                    contract.HandoverStaffId = Guid.Parse(userId);
                }
                contract.IsSignedByCustomer = req.IsSignedByCustomer;
                if (contract.IsSignedByCustomer && contract.IsSignedByStaff)
                {
                    contract.ActualStartDate = DateTimeOffset.UtcNow;
                }
                await _uow.RentalContractRepository.UpdateAsync(contract);
                await _uow.SaveChangesAsync();
                await _uow.CommitAsync();
            }
            catch (Exception)
            {
                await _uow.RollbackAsync();
                throw;
            }
        }

        public async Task<Guid> ReturnProcessRentalContractAsync(ClaimsPrincipal staffClaims, Guid contractId)
        {
            if (!(await VerifyStaffPermission(staffClaims, contractId)))
            {
                throw new ForbidenException(Message.UserMessage.DoNotHavePermission);
            }
            await _uow.BeginTransactionAsync();
            try
            {
                var staffId = staffClaims.FindFirst(JwtRegisteredClaimNames.Sid)!.Value.ToString();
                var contract = await _uow.RentalContractRepository.GetByIdAsync(contractId)
                   ?? throw new NotFoundException(Message.RentalContractMessage.NotFound);
                if (contract.Status == (int)RentalContractStatus.Returned) throw new BusinessException(Message.RentalContractMessage.ContractAlreadyProcess);

                contract.Status = (int)RentalContractStatus.Returned;
                contract.ReturnStaffId = Guid.Parse(staffId);
                var actualEndDate = DateTimeOffset.UtcNow;
                if (contract == null) throw new NotFoundException(Message.RentalContractMessage.NotFound);
                contract.ActualEndDate = actualEndDate;
                var actualLateReturnHours = CalculateLateReturnHours(contract.EndDate, actualEndDate);
                IEnumerable<Invoice> invoices = [];
                Guid returnInvoiceId = Guid.NewGuid();
                var businessVariables = _cache!.Get<List<BusinessVariable>>(Common.SystemCache.BusinessVariables);
                var baseVat = businessVariables!.FirstOrDefault(b => b.Key == (int)BusinessVariableKey.BaseVAT)?.Value;
                var returnInvoice = new Invoice()
                {
                    Id = returnInvoiceId,
                    ContractId = contractId,
                    Status = (int)InvoiceStatus.Pending,
                    Tax = (decimal)baseVat!, //10% dạng decimal
                    Notes = $"GreenWheel – Invoice for your order {contractId}",
                    Type = (int)InvoiceType.Return
                };
                invoices = invoices.Append(returnInvoice);

                IEnumerable<InvoiceItem> returnInvoiceItems = []; //tạo trước invoice item
                var maxLateReturnHours = businessVariables!.FirstOrDefault(b => b.Key == (int)BusinessVariableKey.MaxLateReturnHours)?.Value;
                var lateReturnFeePerHour = businessVariables!.FirstOrDefault(b => b.Key == (int)BusinessVariableKey.LateReturnFeePerHour)?.Value;
                if (actualLateReturnHours > 0)
                {
                    //phí trể giờ
                    returnInvoiceItems = returnInvoiceItems.Append(new InvoiceItem()
                    {
                        InvoiceId = returnInvoiceId,
                        Quantity = (int)actualLateReturnHours,
                        UnitPrice = (decimal)lateReturnFeePerHour!,
                        Type = (int)InvoiceItemType.LateReturn,
                    });
                }
                //phí dọn dẹp
                var cleaningFee = businessVariables!.FirstOrDefault(b => b.Key == (int)BusinessVariableKey.CleaningFee)?.Value;
                returnInvoiceItems = returnInvoiceItems.Append(new InvoiceItem()
                {
                    InvoiceId = returnInvoiceId,
                    Quantity = 1,
                    UnitPrice = (decimal)cleaningFee!,
                    Type = (int)InvoiceItemType.Cleaning,
                });
                returnInvoice.Subtotal = InvoiceHelper.CalculateSubTotalAmount(returnInvoiceItems);
                var vehicle = await _uow.VehicleRepository.GetByIdAsync((Guid)contract.VehicleId!);
                vehicle!.Status = (int)VehicleStatus.Maintenance;
                await _uow.VehicleRepository.UpdateAsync(vehicle);
                await _uow.InvoiceRepository.AddRangeAsync(invoices);
                await _uow.RentalContractRepository.UpdateAsync(contract);
                await _uow.InvoiceItemRepository.AddRangeAsync(returnInvoiceItems);
                await _uow.SaveChangesAsync();
                await _uow.CommitAsync();
                return returnInvoice.Id;
            }
            catch (Exception)
            {
                await _uow.RollbackAsync();
                throw;
            }
        }
        public async Task CancelRentalContract(Guid id, ClaimsPrincipal userClaims)
        {
            var userId = userClaims.FindFirst(JwtRegisteredClaimNames.Sid)!.Value.ToString();
            var contract = await _uow.RentalContractRepository.GetByIdAsync(id)
                ?? throw new NotFoundException(Message.RentalContractMessage.NotFound);
            if(contract.CustomerId != Guid.Parse(userId))
            {
                throw new ForbidenException(Message.UserMessage.DoNotHavePermission);
            }
            contract.Description += "\r\nThe contract was canceled by the customer.";
            if (contract.Status != (int)RentalContractStatus.PaymentPending && contract.Status != (int)RentalContractStatus.RequestPeding)
            {
                throw new BadRequestException(Message.RentalContractMessage.CanNotCancel);
            }
            contract.Status = (int)RentalContractStatus.Cancelled;
            await _uow.RentalContractRepository.UpdateAsync(contract);
        }

        public async Task UpdateStatusAsync(Guid id)
        {
            await _uow.BeginTransactionAsync();
            try
            {
                var contract = await _uow.RentalContractRepository.GetByIdAsync(id)
                ?? throw new NotFoundException(Message.RentalContractMessage.NotFound);

                if (contract.Status == (int)RentalContractStatus.PaymentPending)
                {
                    var invoices = contract.Invoices.Where(i => i.Type == (int)InvoiceType.Reservation || i.Type == (int)InvoiceType.Handover);
                    if (invoices != null && invoices.Any(i => i.Status == (int)InvoiceStatus.Paid && contract.Status == (int)RentalContractStatus.PaymentPending))
                    {
                        contract.Status = (int)RentalContractStatus.Active;
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
                                    await CancelContractAndSendEmail(contract_,
                                     "\r\nBooking was canceled as another customer successfully paid for the same vehicle earlier.");
                                }
                            }
                        }
                    }
                }
                else if (contract.Status == (int)RentalContractStatus.RefundPending)
                {
                    var invoices = contract.Invoices.Where(i => i.Type == (int)InvoiceType.Refund);
                    if (invoices != null && invoices.Any(i => i.Status == (int)InvoiceStatus.Paid))
                    {
                        contract.Status = (int)RentalContractStatus.Completed;
                    }
                }
                await _uow.RentalContractRepository.UpdateAsync(contract);
                await _uow.SaveChangesAsync();
                await _uow.CommitAsync();
            }
            catch (Exception)
            {
                await _uow.RollbackAsync();
                throw;
            }
        }

        public async Task VerifyRentalContract(Guid id, ConfirmReq req, ClaimsPrincipal staffClaims)
        {
            if(!(await VerifyStaffPermission(staffClaims, id)))
            {
                throw new ForbidenException(Message.UserMessage.DoNotHavePermission);
            }
            var rentalContract = await _uow.RentalContractRepository.GetByIdAsync(id)
                ?? throw new NotFoundException(Message.RentalContractMessage.NotFound);
            //Lấy customer
            var customer = (await _uow.RentalContractRepository.GetAllAsync(
                [rc => rc.Customer]))
                .Where(rc => rc.Id == id)
                .Select(rc => rc.Customer).FirstOrDefault();
            if (!rentalContract.VehicleId.HasValue)
                throw new NotFoundException(Message.VehicleMessage.NotFound);
            //var vehicle = await _uow.VehicleRepository.GetByIdAsync((Guid)rentalContract.VehicleId);
            //var vehicleModel = await _uow.VehicleModelRepository.GetByIdAsync(vehicle.ModelId);
            //var station = await _uow.StationRepository.GetByIdAsync(vehicle.StationId);
            var vehicle = await _uow.VehicleRepository.GetByIdAsync((Guid)rentalContract.VehicleId)
                ?? throw new NotFoundException(Message.VehicleMessage.NotFound);
            var vehicleModel = await _uow.VehicleModelRepository.GetByIdAsync(vehicle.ModelId)
                ?? throw new NotFoundException(Message.VehicleModelMessage.NotFound);
            var station = await _uow.StationRepository.GetByIdAsync(vehicle.StationId)
                ?? throw new NotFoundException(Message.StationMessage.NotFound);
            string subject;
            string templatePath;
            string body;
            var basePath = AppContext.BaseDirectory;

            if (rentalContract.Status != (int)RentalContractStatus.RequestPeding)
            {
                throw new BadRequestException(Message.RentalContractMessage.ContractAlreadyProcess);
            }
            await _uow.BeginTransactionAsync();
            try
            {
                if (req.HasVehicle)
                {
                    rentalContract.Status = (int)RentalContractStatus.PaymentPending;
                    await _uow.RentalContractRepository.UpdateAsync(rentalContract);
                    //Lấy invoice
                    var invoice = (await _uow.RentalContractRepository.GetAllAsync(new Expression<Func<RentalContract, object>>[]
                    {
                rc => rc.Invoices
                    })).Where(rc => rc.Id == id)
                    .Select(rc => rc.Invoices).FirstOrDefault();

                    subject = "[GreenWheel] Confirm Your Booking by Completing Payment";
                    templatePath = Path.Combine(basePath, "Templates", "PaymentEmailTemplate.html");
                    body = System.IO.File.ReadAllText(templatePath);

                    var frontendOrigin = Environment.GetEnvironmentVariable("FRONTEND_PUBLIC_ORIGIN")
                            ?? "https://greenwheel.site/";
                    var contractDetailUrl = $"{frontendOrigin}/rental-contracts/{rentalContract.Id}";

                    body = body.Replace("{CustomerName}", customer.LastName + " " + customer.FirstName)
                               .Replace("{BookingId}", rentalContract.Id.ToString())
                               .Replace("{VehicleModelName}", vehicleModel.Name)
                               .Replace("{LisencePlate}", vehicle.LicensePlate)
                               .Replace("{StationName}", station.Name)
                               .Replace("{StartDate}", rentalContract.StartDate.ToString("dd/MM/yyyy"))
                               .Replace("{EndDate}", rentalContract.EndDate.ToString("dd/MM/yyyy"))
                               .Replace("{PaymentLink}", contractDetailUrl);
                }
                else
                {
                    rentalContract.Status = (int)RentalContractStatus.Cancelled;
                    rentalContract.Description += "\r\nThe contract was canceled by the staff due to vehicle unavailability.";
                    await _uow.RentalContractRepository.UpdateAsync(rentalContract);
                    subject = "[GreenWheel] Vehicle Unavailable, Booking Cancelled";
                    templatePath = Path.Combine(basePath, "Templates", "RejectRentalContractEmailTempate.html");
                    body = System.IO.File.ReadAllText(templatePath);
                    if (req.VehicleStatus != null)
                    {
                        vehicle.Status = (int)req.VehicleStatus;
                        await _uow.VehicleRepository.UpdateAsync(vehicle);
                    }
                    body = body.Replace("{CustomerName}", customer.LastName + " " + customer.FirstName)
                               .Replace("{VehicleModelName}", vehicleModel.Name)
                               .Replace("{StationName}", station.Name)
                               .Replace("{StartDate}", rentalContract.StartDate.ToString("dd/MM/yyyy"))
                               .Replace("{EndDate}", rentalContract.EndDate.ToString("dd/MM/yyyy"));
                }
                await _emailService.SendEmailAsync(customer.Email, subject, body);
                await _uow.SaveChangesAsync();
                await _uow.CommitAsync();
            }
            catch (Exception)
            {
                await _uow.RollbackAsync();
                throw;
            }
        }

        public async Task ChangeVehicleAsync(Guid id)
        {
            var contract = await _uow.RentalContractRepository.GetByIdAsync(id)
                ?? throw new NotFoundException(Message.RentalContractMessage.NotFound);
            var returnChecklist = contract.VehicleChecklists
                .FirstOrDefault(c => c.Type == (int)(VehicleChecklistType.Return));

            if (returnChecklist!.MaintainedUntil != null)
            {
                //lấy những hợp đồng có cùng xe với hợp đồng này mà có trạng thái là đang active
                var otherContracts = await _uow.RentalContractRepository.GetByVehicleIdAsync((Guid)contract.VehicleId!);
                otherContracts = otherContracts != null ? otherContracts.Where(c => c.Id != contract.Id
                                            && (c.Status == (int)RentalContractStatus.Active
                                                || c.Status == (int)RentalContractStatus.RequestPeding
                                                || c.Status == (int)RentalContractStatus.PaymentPending)) : null;
                //nếu có hợp đồng cùng xe thì tục
                if (otherContracts != null)
                {
                    IEnumerable<RentalContract> flagContract = [];
                    foreach (var contract_ in otherContracts)
                    {
                        if (contract.StartDate <= returnChecklist.MaintainedUntil)
                        {
                            flagContract = flagContract.Append(contract_);
                        }
                    }
                    if (flagContract.Any())
                    {
                        await _uow.BeginTransactionAsync();
                        try
                        {
                            foreach (var contract_ in flagContract)
                            {
                                if (contract_.Status == (int)RentalContractStatus.RequestPeding
                                    || contract_.Status == (int)RentalContractStatus.PaymentPending)
                                {
                                    await CancelContractAndSendEmail(contract_,
                                        "\r\nBooking was canceled because vehicle was maintained");
                                }
                                else if (contract_.Status == (int)RentalContractStatus.Active)
                                {
                                    contract_.Status = (int)RentalContractStatus.UnavailableVehicle;
                                    var model = await _uow.VehicleModelRepository.GetByIdAsync(contract_.Vehicle!.ModelId
                                            , contract_.StationId, contract_.StartDate, contract_.EndDate)
                                        ?? throw new NotFoundException(Message.VehicleModelMessage.NotFound);
                                    var vehicle = model.Vehicles == null ? null : model.Vehicles.FirstOrDefault();
                                    contract_.VehicleId = null;
                                    if (vehicle != null)
                                    {
                                        contract_.VehicleId = vehicle.Id;
                                    }
                                    var subject = "[GreenWheel] Issue Detected in Your GreenWheel Rental Contract";
                                    var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "VehicleIssueNotification.html");
                                    var body = System.IO.File.ReadAllText(templatePath);
                                    var customer = contract_.Customer;
                                    if (customer.Email != null)
                                    {
                                        var station = contract_.Station;
                                        var vehicleToCancel = contract_.Vehicle
                                            ?? throw new NotFoundException(Message.VehicleMessage.NotFound);
                                        var frontendOrigin = Environment.GetEnvironmentVariable("FRONTEND_PUBLIC_ORIGIN")
                                                            ?? "https://greenwheel.site/";
                                        var contractDetailUrl = $"{frontendOrigin}";

                                        body = body = body.Replace("{CustomerName}", $"{customer.LastName} {customer.FirstName}")
                                                   .Replace("{ContractCode}", contract_.Id.ToString())
                                                   .Replace("{VehicleName}", model.Name)
                                                   .Replace("{LisencePlate}", vehicleToCancel.LicensePlate)
                                                   .Replace("{StationName}", station.Name)
                                                   .Replace("{StartDate}", contract_.StartDate.ToString("dd/MM/yyyy"))
                                                   .Replace("{EndDate}", contract_.EndDate.ToString("dd/MM/yyyy"))
                                                   .Replace("{ResolveLink}", contractDetailUrl);

                                        await _emailService.SendEmailAsync(customer.Email!, subject, body);
                                    }
                                    await _uow.RentalContractRepository.UpdateAsync(contract_);
                                }
                            }
                            await _uow.SaveChangesAsync();
                            await _uow.CommitAsync();
                        }
                        catch (Exception)
                        {
                            await _uow.RollbackAsync();
                            throw;
                        }
                    }
                }
            }
        }

        public async Task CancelContractAndSendEmail(RentalContract contract_, string description)
        {
            contract_.Status = (int)RentalContractStatus.Cancelled;
            contract_.Description += "\r\n" + description;
            var subject = "[GreenWheel] Your Booking Has Been Canceled";
            var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "CancelAutoEmailTemplate.html");
            var body = System.IO.File.ReadAllText(templatePath);
            var customer = contract_.Customer;
            if (customer.Email != null)
            {
                var station = contract_.Station;
                var vehicleToCancel = contract_.Vehicle
                    ?? throw new NotFoundException(Message.VehicleMessage.NotFound);
                var model = vehicleToCancel.Model;

                var frontendOrigin = Environment.GetEnvironmentVariable("FRONTEND_PUBLIC_ORIGIN")
                            ?? "https://greenwheel.site/";
                var contractDetailUrl = $"{frontendOrigin}/vehicle-models";

                body = body.Replace("{CustomerName}", $"{customer.LastName} {customer.FirstName}")
                           .Replace("{ContractCode}", contract_.Id.ToString())
                           .Replace("{VehicleName}", model.Name)
                           .Replace("{LisencePlate}", vehicleToCancel.LicensePlate)
                           .Replace("{StationName}", station.Name)
                           .Replace("{StartDate}", contract_.StartDate.ToString("dd/MM/yyyy"))
                           .Replace("{EndDate}", contract_.EndDate.ToString("dd/MM/yyyy"))
                           .Replace("{BookingLink}", contractDetailUrl);
                await _emailService.SendEmailAsync(customer.Email!, subject, body);
            }
            await _uow.RentalContractRepository.UpdateAsync(contract_);
        }

        public async Task ProcessCustomerConfirm(Guid id, int resolutionOption, ClaimsPrincipal userClaims)
        {
            var userId = userClaims.FindFirst(JwtRegisteredClaimNames.Sid)!.Value.ToString();
            var contract = await _uow.RentalContractRepository.GetByIdAsync(id)
                ?? throw new NotFoundException(Message.RentalContractMessage.NotFound);
            if (Guid.Parse(userId) != contract.CustomerId)
            {
                throw new ForbidenException(Message.UserMessage.DoNotHavePermission);
            }
            if (contract.Status != (int)RentalContractStatus.UnavailableVehicle)
            {
                throw new BadRequestException(Message.RentalContractMessage.ContractAlreadyProcess);
            }
            await _uow.BeginTransactionAsync();
            try
            {
                if (resolutionOption == (int)VehicleIssueResolutionOption.ChangeVehicle)
                {
                    contract.Status = (int)RentalContractStatus.Active;
                }
                else
                {
                    contract.Status = (int)RentalContractStatus.RefundPending;
                    var subject = "[GreenWheel] Your Booking Has Been Canceled";
                    var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "RefundEmailTemplate.html");
                    var body = System.IO.File.ReadAllText(templatePath);
                    var customer = contract.Customer;
                    if (customer.Email != null)
                    {
                        body = body.Replace("{CustomerName}", $"{customer.LastName} {customer.FirstName}")
                               .Replace("{ContractCode}", contract.Id.ToString());

                        await _emailService.SendEmailAsync(customer.Email!, subject, body);
                    }
                    var invoiceId = Guid.NewGuid();
                    var invoice = new Invoice()
                    {
                        Id = invoiceId,
                        ContractId = contract.Id,
                        Status = (int)InvoiceStatus.Pending,
                        Tax = Common.Tax.NoneVAT, //10% dạng decimal
                        Notes = $"GreenWheel – Invoice for your order {contract.Id}",
                        Type = (int)InvoiceType.Refund
                    };
                    var item = new InvoiceItem()
                    {
                        InvoiceId = invoiceId,
                        Quantity = 1,
                        UnitPrice = 0,
                        Description = $"Refund for order {contract.Id}",
                        Type = (int)InvoiceItemType.Refund,
                    };
                    var handoverInvoice = contract.Invoices.FirstOrDefault(i => i.Type == (int)InvoiceType.Handover);
                    var reservation = contract.Invoices.FirstOrDefault(i => i.Type == (int)InvoiceType.Reservation);
                    if (handoverInvoice!.Status == (int)InvoiceStatus.Paid)
                    {
                        item.UnitPrice += (decimal)handoverInvoice.PaidAmount!;
                    }
                    if (reservation!.Status == (int)InvoiceStatus.Paid)
                    {
                        item.UnitPrice += (decimal)reservation.PaidAmount!;
                    }
                    await _uow.InvoiceRepository.AddAsync(invoice);
                    await _uow.InvoiceItemRepository.AddAsync(item);
                }
                await _uow.RentalContractRepository.UpdateAsync(contract);
                await _uow.SaveChangesAsync();
                await _uow.CommitAsync();
            }
            catch (Exception)
            {
                await _uow.RollbackAsync();
                throw;
            }
        }

        public async Task<PageResult<RentalContractViewRes>> GetAllByPagination(
            GetAllRentalContactReq req, PaginationParams pagination)
        {
            var pageResult = await _uow.RentalContractRepository.GetAllByPaginationAsync(
                req.Status,
                req.Phone,
                req.CitizenIdentityNumber,
                req.DriverLicenseNumber,
                req.StationId,
                pagination);

            var mapped = _mapper.Map<IEnumerable<RentalContractViewRes>>(pageResult.Items);

            return new PageResult<RentalContractViewRes>(
                mapped,
                pageResult.PageNumber,
                pageResult.PageSize,
                pageResult.Total
            );
        }

        public async Task<PageResult<RentalContractViewRes>> GetMyContractsByPagination(
            ClaimsPrincipal user,
            PaginationParams pagination,
            int? status, Guid? stationId)
        {
            var customerId = Guid.Parse(user.FindFirstValue(JwtRegisteredClaimNames.Sid)!);

            var result = await _uow.RentalContractRepository
                .GetMyContractsAsync(customerId, pagination, status, stationId);

            var mapped = _mapper.Map<IEnumerable<RentalContractViewRes>>(result.Items);

            return new PageResult<RentalContractViewRes>(
                mapped,
                result.PageNumber,
                result.PageSize,
                result.Total
            );
        }
        private int CalculateLateReturnHours(DateTimeOffset expectedReturnDate, DateTimeOffset actualReturnDate)
        {
            var businessVariables = _cache!.Get<List<BusinessVariable>>(Common.SystemCache.BusinessVariables);
            var maxLateReturnHour = businessVariables!.FirstOrDefault(b => b.Key == (int)BusinessVariableKey.MaxLateReturnHours)!.Value;
            var totalHoursLate = (actualReturnDate - expectedReturnDate).TotalHours;
            totalHoursLate = Math.Ceiling(totalHoursLate);
            totalHoursLate -= (int)maxLateReturnHour!;
            return (int)totalHoursLate;
        }

        public async Task LateReturnContractWarningAsync()
        {
            var targetContracts = await _uow.RentalContractRepository.GetLateReturnContract();
            if (targetContracts == null || !targetContracts.Any())
            {
                return;
            }
            var subject = "[GreenWheel] Vehicle Return Overdue Notice – Immediate Attention Required";
            await _uow.BeginTransactionAsync();
            try
            {
                foreach (var contract in targetContracts)
                {
                    var customer = contract.Customer;
                    var model = contract.Vehicle!.Model;
                    var vehicle = contract.Vehicle;
                    var station = contract.Station;
                    var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "LateReturnEmailTemplate.html");
                    var body = System.IO.File.ReadAllText(templatePath);
                    body = body.Replace("{CustomerName}", $"{customer.LastName} {customer.FirstName}")
                           .Replace("{BookingId}", contract.Id.ToString())
                           .Replace("{VehicleModelName}", model.Name)
                           .Replace("{LicensePlate}", vehicle.LicensePlate)
                           .Replace("{StationName}", station.Name)
                           .Replace("{EndDate}", contract.EndDate.ToString("dd/MM/yyyy"));
                    if (customer.Email != null)
                    {
                        await _emailService.SendEmailAsync(customer.Email!, subject, body);
                    }
                    vehicle.Status = (int)VehicleStatus.LateReturn;
                    await _uow.VehicleRepository.UpdateAsync(vehicle);
                }
                await _uow.SaveChangesAsync();
                await _uow.CommitAsync();
            }
            catch (Exception)
            {
                await _uow.RollbackAsync();
                throw;
            }
        }

        public async Task ExpiredContractCleanUpAsync()
        {
            var targetContracts = await _uow.RentalContractRepository.GetExpiredContractAsync();
            if (targetContracts == null || !targetContracts.Any())
            {
                return;
            }
            var subject = "[GreenWheel] Rental Contract Cancelled – Pickup Deadline Missed";
            await _uow.BeginTransactionAsync();
            try
            {
                foreach (var contract in targetContracts)
                {
                    var customer = contract.Customer;
                    var model = contract.Vehicle!.Model;
                    var vehicle = contract.Vehicle;
                    var station = contract.Station;
                    var templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "CancelExpiredContractEmailTemplate.html");
                    var body = System.IO.File.ReadAllText(templatePath);
                    body = body.Replace("{CustomerName}", $"{customer.LastName} {customer.FirstName}")
                                    .Replace("{BookingId}", contract.Id.ToString())
                                    .Replace("{VehicleModelName}", model.Name)
                                    .Replace("{LicensePlate}", vehicle.LicensePlate)
                                    .Replace("{StationName}", station.Name)
                                    .Replace("{EndDate}", contract.EndDate.ToString("dd/MM/yyyy"));

                    await _emailService.SendEmailAsync(customer.Email!, subject, body);
                    if (contract.Status == (int)RentalContractStatus.Active)
                    {
                        var ortherContracts = (await _uow.RentalContractRepository.GetByVehicleIdAsync((Guid)contract.VehicleId!))
                                                        .Where(r => r.Id != contract.Id && r.Status == (int)RentalContractStatus.Active);
                        if (!ortherContracts.Any())
                        {
                            vehicle.Status = (int)VehicleStatus.Available;
                            await _uow.VehicleRepository.UpdateAsync(vehicle);
                        }
                    }
                    contract.Status = (int)RentalContractStatus.Cancelled;
                    contract.Description += "\r\nCustomer did not pick up the vehicle before the scheduled deadline.";
                    await _uow.RentalContractRepository.UpdateAsync(contract);
                }
                await _uow.SaveChangesAsync();
                await _uow.CommitAsync();
            }
            catch (Exception)
            {
                await _uow.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> VerifyStaffPermission(ClaimsPrincipal staffClaims, Guid contractId)
        {
            if (Guid.TryParse(staffClaims.FindFirst(JwtRegisteredClaimNames.Sid)!.Value.ToString(), out var staffId))
            {
                var staff = await _staffRepository.GetByUserIdAsync(staffId) 
                    ?? throw new NotFoundException(Message.UserMessage.NotFound);
                var contract = await _uow.RentalContractRepository.GetByIdAsync(contractId)
                    ?? throw new NotFoundException(Message.RentalContractMessage.NotFound);
                if(staff.StationId == contract.StationId)
                {
                    return true;
                }
            };
            return false;
        }
    }
}