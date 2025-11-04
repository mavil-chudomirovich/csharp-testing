using Application.Dtos.Common.Request;
using Application.Dtos.Common.Response;
using Application.Dtos.Invoice.Response;
using Application.Dtos.RentalContract.Request;
using Application.Dtos.RentalContract.Respone;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions
{
    public interface IRentalContractService
    {
        Task CreateRentalContractAsync(Guid UserID, CreateRentalContractReq createRentalContractReq);
         Task VerifyRentalContract(Guid id, ConfirmReq req, ClaimsPrincipal staffClaims);
        Task UpdateStatusAsync(Guid id);
        Task<RentalContractViewRes> GetByIdAsync(Guid id);

        //Task<IEnumerable<RentalContractViewRes>> GetAll(GetAllRentalContactReq req);
        Task<PageResult<RentalContractViewRes>> GetAllByPagination(GetAllRentalContactReq req, PaginationParams pagination);

        Task HandoverProcessRentalContractAsync(ClaimsPrincipal staffClaims, Guid id, HandoverContractReq req);
        Task<Guid> ReturnProcessRentalContractAsync(ClaimsPrincipal staffClaims, Guid id);

        //Task<IEnumerable<RentalContractViewRes>> GetMyContracts(ClaimsPrincipal userClaims, int? status);
        Task<PageResult<RentalContractViewRes>> GetMyContractsByPagination(
            ClaimsPrincipal user, PaginationParams pagination, int? status, Guid? stationId);

        Task CancelRentalContract(Guid id, ClaimsPrincipal userClaims);
        Task ChangeVehicleAsync(Guid id);
        Task ProcessCustomerConfirm(Guid id, int resolutionOption, ClaimsPrincipal userClaims);
        Task LateReturnContractWarningAsync();
        Task ExpiredContractCleanUpAsync();
        Task CancelContractAndSendEmail(RentalContract contract_, string description);
        Task<bool> VerifyStaffPermission(ClaimsPrincipal staffClaims, Guid contractId);
    }
}
