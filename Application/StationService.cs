using Application.Abstractions;
using Application.AppExceptions;
using Application.Constants;
using Application.Dtos.Station.Request;
using Application.Dtos.Station.Respone;
using Application.Repositories;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Google.Apis.Requests.RequestError;

namespace Application
{
    public class StationService : IStationService
    {
        private readonly IStationRepository _stationRepository;
        private readonly IMapper _mapper;
        public StationService(IStationRepository stationRepository, IMapper mapper)
        {
            _stationRepository = stationRepository;
            _mapper = mapper;
        }
        public async Task<IEnumerable<StationViewRes>> GetAllStation()
        {
            var stations = await _stationRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<StationViewRes>>(stations) ?? [];
        }
        public async Task<StationViewRes?> GetByIdAsync(Guid id)
        {
            var station = await _stationRepository.GetByIdAsync(id);
            if (station == null || station.DeletedAt != null)
                throw new NotFoundException(Message.StationMessage.NotFound);

            return _mapper.Map<StationViewRes>(station);
        }

        public async Task<StationViewRes> CreateAsync(StationCreateReq request)
        {
            var entity = _mapper.Map<Station>(request);
            entity.Id = Guid.NewGuid();

            await _stationRepository.AddAsync(entity);
            return _mapper.Map<StationViewRes>(entity);
        }

        public async Task<StationViewRes> UpdateAsync(StationUpdateReq request)
        {
            var station = await _stationRepository.GetByIdAsync(request.Id);
            if (station == null || station.DeletedAt != null)
                throw new NotFoundException(Message.StationMessage.NotFound);

            station.Name = request.Name ?? station.Name;
            station.Address = request.Address ?? station.Address;

            await _stationRepository.UpdateAsync(station);
            return _mapper.Map<StationViewRes>(station);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _stationRepository.DeleteAsync(id);
        }
    }
}

