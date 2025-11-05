using Application.Dtos.Brand.Request;
using Application.Dtos.Brand.Respone;
using Application.Dtos.Station.Respone;
using Application.Dtos.VehicleModel.Request;
using Application.Dtos.VehicleModel.Respone;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Abstractions
{
    public interface IBrandService
    {
        Task<IEnumerable<BrandViewRes>> GetAllAsync();
        Task<BrandViewRes> GetByIdAsync(Guid id);
        Task<BrandViewRes> CreateAsync(BrandReq dto);
        Task<BrandViewRes> UpdateAsync(Guid id, UpdateBrandReq dto);
        Task DeleteAsync(Guid id);
    }
}
