using Application.Abstractions;
using Application.AppExceptions;
using Application.Constants;
using Application.Dtos.Brand.Request;
using Application.Dtos.Brand.Respone;
using Application.Repositories;
using AutoMapper;
using Domain.Entities;

namespace Application
{
    public class BrandService : IBrandService
    {
        private readonly IBrandRepository _repo;
        private readonly IMapper _mapper;
        public BrandService(IBrandRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<BrandViewRes> CreateAsync(BrandReq dto)
        {
            var brand = _mapper.Map<Brand>(dto);
            brand.Id = Guid.NewGuid();
            await _repo.AddAsync(brand);
            return _mapper.Map<BrandViewRes>(brand);

        }

        public async Task DeleteAsync(Guid id)
        {
            await _repo.DeleteAsync(id);
        }

        public async Task<IEnumerable<BrandViewRes>> GetAllAsync()
        {
            var brand = await _repo.GetAllAsync();
            return _mapper.Map<IEnumerable<BrandViewRes>>(brand);
        }

        public async Task<BrandViewRes> GetByIdAsync(Guid id)
        {
            var brand = await _repo.GetByIdAsync(id) ?? throw new BusinessException(Message.BrandMessage.NotFound);
            return _mapper.Map<BrandViewRes>(brand);
        }

        public async Task<BrandViewRes> UpdateAsync(Guid id, UpdateBrandReq dto)
        {
            var brand = await _repo.GetByIdAsync(id) ?? throw new BusinessException(Message.BrandMessage.NotFound);
            _mapper.Map(dto, brand);
            await _repo.UpdateAsync(brand);
            return _mapper.Map<BrandViewRes>(brand);
        }
    }
}
