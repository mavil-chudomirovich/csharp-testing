using Application.Dtos.Common.Request;
using Application.Dtos.Common.Response;
using Application.Dtos.VehicleComponent.Request;
using Application.Dtos.VehicleComponent.Respone;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions
{
    public interface IVehicleComponentService
    {
        Task DeleteAsync(Guid id);
        Task<Guid> AddAsync(CreateVehicleComponentReq req);
        Task UpdateAsync(Guid id, UpdateVehicleComponentReq req);
        Task<PageResult<VehicleComponentViewRes>> GetAllAsync(Guid? id, string? name, PaginationParams pagination);
        Task<VehicleComponentViewRes> GetByIdAsync(Guid id);
    }
}
