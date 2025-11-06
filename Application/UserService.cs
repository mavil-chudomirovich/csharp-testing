using Application.Abstractions;
using Application.AppExceptions;
using Application.Constants;
using Application.Dtos.Common.Request;
using Application.Dtos.Common.Response;
using Application.Dtos.User.Request;
using Application.Dtos.User.Respone;
using Application.Repositories;
using AutoMapper;
using Domain.Entities;
using Microsoft.Extensions.Caching.Memory;

namespace Application
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ICitizenIdentityRepository _citizenIdentityRepository;
        private readonly IDriverLicenseRepository _driverLicenseRepository;
        private readonly IMemoryCache _cache;
        private readonly IStaffRepository _staffRepository;

        public UserService(IUserRepository repository,
             IMapper mapper,
             ICitizenIdentityRepository citizenIdentityRepository,
             IDriverLicenseRepository driverLicenseRepository,
             IMemoryCache cache,
             IStaffRepository staffRepository
            )
        {
            _userRepository = repository;
            _mapper = mapper;
            _citizenIdentityRepository = citizenIdentityRepository;
            _driverLicenseRepository = driverLicenseRepository;
            _cache = cache;
            _staffRepository = staffRepository;
        }

        public async Task<Guid> CreateAsync(CreateUserReq req)
        {
            if (await _userRepository.GetByPhoneAsync(req.Phone) != null)
                throw new ConflictDuplicateException(Message.UserMessage.PhoneAlreadyExist);

            if (!string.IsNullOrEmpty(req.Email) && await _userRepository.GetByEmailAsync(req.Email) != null)
                throw new ConflictDuplicateException(Message.UserMessage.EmailAlreadyExists);

            if (req.RoleName != null && req.RoleName != RoleName.Customer && req.StationId == null)
                throw new BadRequestException(Message.UserMessage.StationIdIsRequired);

            if (string.IsNullOrEmpty(req.RoleName))
                req.RoleName = RoleName.Customer;

            var user = _mapper.Map<User>(req);
            var roles = _cache.Get<List<Role>>(Common.SystemCache.AllRoles);
            var userRoleId = roles.FirstOrDefault(r => string.Compare(r.Name, req.RoleName, StringComparison.OrdinalIgnoreCase) == 0)!.Id;
            user.RoleId = userRoleId;
            await _userRepository.AddAsync(user);

            if (req.RoleName == RoleName.Customer) return user.Id;

            var staff = new Staff
            {
                UserId = user.Id,
                //StationId = (Guid)req.StationId,
                StationId = req.StationId!.Value,
            };
            await _staffRepository.AddAsync(staff);
            return user.Id;
        }

        public async Task<PageResult<UserProfileViewRes>> GetAllWithPaginationAsync(PaginationParams pagination,
            string? phone, string? citizenIdNumber, string? driverLicenseNumber, string? roleName, Guid? stationId
        )
        {
            var pageResult = await _userRepository.GetAllWithPaginationAsync(pagination,
                phone, citizenIdNumber, driverLicenseNumber, roleName, stationId);

            var mapped = _mapper.Map<IEnumerable<UserProfileViewRes>>(pageResult.Items);

            return new PageResult<UserProfileViewRes>(
                mapped,
                pageResult.PageNumber,
                pageResult.PageSize,
                pageResult.Total
            );
        }

        public async Task<UserProfileViewRes?> GetByIdAsync(Guid id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return _mapper.Map<UserProfileViewRes>(user);
        }

        public async Task<UserProfileViewRes> GetByPhoneAsync(string phone)
        {
            var user = await _userRepository.GetByPhoneAsync(phone);
            if (user == null)
            {
                throw new NotFoundException(Message.UserMessage.NotFound);
            }
            var userViewRes = _mapper.Map<UserProfileViewRes>(user);
            return userViewRes;
        }

        public async Task<UserProfileViewRes> GetByCitizenIdentityAsync(string idNumber)
        {
            var citizenIdentity = await _citizenIdentityRepository.GetByIdNumberAsync(idNumber);
            if (citizenIdentity == null)
            {
                throw new NotFoundException(Message.UserMessage.CitizenIdentityNotFound);
            }
            var userView = _mapper.Map<UserProfileViewRes>(citizenIdentity.User);
            return userView;
        }

        public async Task<UserProfileViewRes> GetByDriverLicenseAsync(string number)
        {
            var driverLicense = await _driverLicenseRepository.GetByLicenseNumber(number);
            if (driverLicense == null)
            {
                throw new NotFoundException(Message.UserMessage.CitizenIdentityNotFound);
            }
            var userView = _mapper.Map<UserProfileViewRes>(driverLicense.User);
            return userView;
        }

        public async Task<PageResult<UserProfileViewRes>> GetAllStaffAsync(PaginationParams pagination, string? name, Guid? stationId)
        {
            var pageResult = await _userRepository.GetAllStaffAsync(pagination, name, stationId);
            var mapped = _mapper.Map<IEnumerable<UserProfileViewRes>>(pageResult.Items);

            return new PageResult<UserProfileViewRes>(
                mapped,
                pageResult.PageNumber,
                pageResult.PageSize,
                pageResult.Total
            );
        }

        public async Task DeleteCustomer(Guid id)
        {
            await _userRepository.DeleteAsync(id);
        }
    }
}