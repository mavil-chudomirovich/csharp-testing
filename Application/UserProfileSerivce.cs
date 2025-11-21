using Application.Abstractions;
using Application.AppExceptions;
using Application.Constants;
using Application.Dtos.CitizenIdentity.Request;
using Application.Dtos.CitizenIdentity.Response;
using Application.Dtos.Common.Request;
using Application.Dtos.DriverLicense.Request;
using Application.Dtos.DriverLicense.Response;
using Application.Dtos.User.Request;
using Application.Dtos.User.Respone;
using Application.Helpers;
using Application.Repositories;
using Application.UnitOfWorks;
using AutoMapper;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Application
{
    public class UserProfileSerivce : IUserProfileSerivce
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        private readonly ICitizenIdentityService _citizenService;
        private readonly IDriverLicenseService _driverService;
        private readonly IMediaUow _mediaUow;
        private readonly ICitizenIdentityRepository _citizenIdentityRepository;
        private readonly IDriverLicenseRepository _driverLicenseRepository;

        public UserProfileSerivce(IUserRepository repository,
             IMapper mapper,
             IPhotoService photoService,
             ICitizenIdentityService citizenService,
             IDriverLicenseService driverService,
             IMediaUow mediaUow,
             ICitizenIdentityRepository citizenIdentityRepository,
             IDriverLicenseRepository driverLicenseRepository
            )
        {
            _userRepository = repository;
            _mapper = mapper;
            _photoService = photoService;
            _citizenService = citizenService;
            _driverService = driverService;
            _mediaUow = mediaUow;
            _citizenIdentityRepository = citizenIdentityRepository;
            _driverLicenseRepository = driverLicenseRepository;
        }

        // ===========================
        // Profile
        // ===========================

        public async Task<UserProfileViewRes> GetMeAsync(ClaimsPrincipal userClaims)
        {
            //Guid userID = Guid.Parse(userClaims.FindFirst(JwtRegisteredClaimNames.Sid).Value.ToString());
            var sidClaim = userClaims.FindFirst(JwtRegisteredClaimNames.Sid);
            if (sidClaim == null || string.IsNullOrEmpty(sidClaim.Value))
                throw new NotFoundException(Message.UserMessage.NotFound);
            Guid userID = Guid.Parse(sidClaim.Value);
            //User userFromDb = await _userRepository.GetByIdAsync(userID);
            // Lấy hồ sơ người dùng KÈM theo thông tin Role (Phúc thêm)
            // Mục đích: khi trả về UserProfileViewRes cần có tên/quyền của vai trò (vd: "Customer", "Staff")
            // Lý do: tránh phải query thêm để lấy Role, đồng thời đảm bảo mapping có đủ dữ liệu quyền hạn
            // added: include role data when retrieving staff profile
            // Mục đích:  response /api/users/me trả về đầy đủ thông tin role,
            // giúp useAuth ở frontend biết chắc user có role “staff”.
            User? userFromDb = await _userRepository.GetByIdWithFullInfoAsync(userID)
                ?? throw new NotFoundException(Message.UserMessage.NotFound);
            return _mapper.Map<UserProfileViewRes>(userFromDb);
        }

        public async Task UpdateAsync(Guid userId, UserUpdateReq req)
        {
            if (!string.IsNullOrEmpty(req.Phone))
            {
                var existingUser = await _userRepository.GetByPhoneAsync(req.Phone);
                if (existingUser != null && existingUser.Id != userId)
                    throw new ConflictDuplicateException(Message.UserMessage.PhoneAlreadyExist);
            }
            User userFromDb = await _userRepository.GetByIdAsync(userId)
                ?? throw new DirectoryNotFoundException(Message.UserMessage.NotFound);

            if (req.FirstName != null) userFromDb.FirstName = req.FirstName;
            if (req.LastName != null) userFromDb.LastName = req.LastName;
            if (!string.IsNullOrEmpty(req.Phone)) userFromDb.Phone = req.Phone;
            if (req.DateOfBirth != null) userFromDb.DateOfBirth = req.DateOfBirth;
            if (req.Sex != null) userFromDb.Sex = req.Sex;
            if (!string.IsNullOrEmpty(req.AvatarUrl)) userFromDb.AvatarUrl = req.AvatarUrl;
            if (req.HasSeenTutorial != null) userFromDb.HasSeenTutorial = (bool)req.HasSeenTutorial;
            await _userRepository.UpdateAsync(userFromDb);
        }


        public async Task<string> UploadAvatarAsync(Guid userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException(Message.CloudinaryMessage.NotFoundObjectInFile);

            var uploadReq = new UploadImageReq { File = file };
            var uploaded = await _photoService.UploadPhotoAsync(uploadReq, $"users/{userId}");
            if (string.IsNullOrEmpty(uploaded.Url))
                throw new InvalidOperationException(Message.CloudinaryMessage.UploadFailed);

            var user = await _mediaUow.Users.GetByIdAsync(userId)
                ?? throw new KeyNotFoundException(Message.UserMessage.NotFound);
            var oldPublicId = user.AvatarPublicId;
            var result = await _photoService.UploadPhotoAsync(uploadReq, $"users/{userId}");

            if (string.IsNullOrEmpty(result.Url))
                throw new InvalidOperationException(Message.CloudinaryMessage.UploadFailed);

            await using var trx = await _mediaUow.BeginTransactionAsync();
            try
            {
                user.AvatarUrl = uploaded.Url;
                user.AvatarPublicId = uploaded.PublicID;
                user.UpdatedAt = DateTime.UtcNow;

                await _mediaUow.Users.UpdateAsync(user);
                await _mediaUow.SaveChangesAsync();
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

            return user.AvatarUrl!;
        }

        public async Task DeleteAvatarAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId)
                ?? throw new Exception(Message.UserMessage.NotFound);

            if (string.IsNullOrEmpty(user.AvatarPublicId))
                throw new Exception(Message.UserMessage.AvatarNotFound);
            if( user.AvatarPublicId != null)
                await _photoService.DeletePhotoAsync(user.AvatarPublicId);

            user.AvatarUrl = null;
            user.AvatarPublicId = null;
            await _userRepository.UpdateAsync(user);
        }

        public async Task CheckDupEmailAsync(string email)
        {
            if (await _userRepository.GetByEmailAsync(email) != null)
            {
                throw new ConflictDuplicateException(Message.UserMessage.EmailAlreadyExists);
            }
        }

        public async Task<CitizenIdentityRes> UploadCitizenIdAsync(Guid userId, UploadImagesReq req)
        {
            if (req.Files.Count != 2)
            {
                throw new BadRequestException(Message.UserMessage.InvalidCitizenIdentityImagesAmount);
            }

            var frontUploadReq = new UploadImageReq { File = req.Files[0] };
            var frontUploaded = await _photoService.UploadPhotoAsync(frontUploadReq, "citizen-ids-front");
            var backUploadReq = new UploadImageReq { File = req.Files[1] };
            var backUploaded = await _photoService.UploadPhotoAsync(backUploadReq, "citizen-ids-back");
            if (string.IsNullOrEmpty(frontUploaded.Url) || string.IsNullOrEmpty(backUploaded.Url))
                throw new InvalidOperationException(Message.CloudinaryMessage.UploadFailed);
            var docType = await _citizenService.VerifyDocumentTypeAsync(frontUploaded.Url);
            if (docType != "CitizenID")
            {
                await _photoService.DeletePhotoAsync(frontUploaded.PublicID);
                await _photoService.DeletePhotoAsync(backUploaded.PublicID);
                throw new BadRequestException(Message.UserMessage.InvalidCitizenIdDocumentType);
            }
            var old = await _mediaUow.CitizenIdentities.GetByUserIdAsync(userId);

            await using var trx = await _mediaUow.BeginTransactionAsync();
            try
            {
                var entity = await _citizenService.ProcessCitizenIdentityAsync(userId,
                    frontUploaded.Url, frontUploaded.PublicID, backUploaded.Url, backUploaded.PublicID)
                    ?? throw new BusinessException(Message.UserMessage.InvalidCitizenIdData);

                await _mediaUow.SaveChangesAsync();
                await trx.CommitAsync();

                if (!string.IsNullOrEmpty(old?.FrontImagePublicId))
                {
                    try 
                    { 
                        await _photoService.DeletePhotoAsync(old.FrontImagePublicId);
                        await _photoService.DeletePhotoAsync(backUploaded.PublicID);
                    } catch { }
                }

                return _mapper.Map<CitizenIdentityRes>(entity);
            }
            catch
            {
                await trx.RollbackAsync();
                try 
                { 
                    await _photoService.DeletePhotoAsync(frontUploaded.PublicID);
                    await _photoService.DeletePhotoAsync(backUploaded.PublicID);
                }
                catch { }
                throw;
            }
        }

        public async Task<DriverLicenseRes> UploadDriverLicenseAsync(Guid userId, UploadImagesReq req)
        {
            if (req.Files.Count != 2)
            {
                throw new BadRequestException(Message.UserMessage.InvalidDriverLicenseImagesAmount);
            }

            var frontUploadReq = new UploadImageReq { File = req.Files[0] };
            var frontUploaded = await _photoService.UploadPhotoAsync(frontUploadReq, "driver-license-front");
            var backUploadReq = new UploadImageReq { File = req.Files[1] };
            var backUploaded = await _photoService.UploadPhotoAsync(backUploadReq, "driver-license-back");
            if (string.IsNullOrEmpty(frontUploaded.Url) || string.IsNullOrEmpty(backUploaded.Url))
                throw new InvalidOperationException(Message.CloudinaryMessage.UploadFailed);
            var docType = await _driverService.VerifyDocumentTypeAsync(frontUploaded.Url);
            if (docType != "DriverLicense")
            {
                await _photoService.DeletePhotoAsync(frontUploaded.PublicID);
                await _photoService.DeletePhotoAsync(backUploaded.PublicID);
                throw new BadRequestException(Message.UserMessage.InvalidDriverLicenseDocumentType);
            }
            var old = await _mediaUow.DriverLicenses.GetByUserIdAsync(userId);

            await using var trx = await _mediaUow.BeginTransactionAsync();
            try
            {
                var entity = await _driverService.ProcessDriverLicenseAsync(userId,
                    frontUploaded.Url, frontUploaded.PublicID, backUploaded.Url, backUploaded.PublicID)
                    ?? throw new BusinessException(Message.UserMessage.InvalidDriverLicenseData);

                await _mediaUow.SaveChangesAsync();
                await trx.CommitAsync();

                if (!string.IsNullOrEmpty(old?.FrontImagePublicId))
                {
                    try 
                    { 
                        await _photoService.DeletePhotoAsync(old.FrontImagePublicId);
                        await _photoService.DeletePhotoAsync(backUploaded.PublicID);
                    } catch { }
                }

                return _mapper.Map<DriverLicenseRes>(entity);
            }
            catch
            {
                await trx.RollbackAsync();
                try { 
                    await _photoService.DeletePhotoAsync(frontUploaded.PublicID); 
                    await _photoService.DeletePhotoAsync(backUploaded.PublicID); 
                } catch { }
                throw;
            }
        }

        public async Task<CitizenIdentityRes?> GetMyCitizenIdentityAsync(Guid userId)
        {
            var entity = await _citizenService.GetByUserId(userId);
            if (entity == null)
                throw new NotFoundException(Message.UserMessage.CitizenIdentityNotFound);

            return _mapper.Map<CitizenIdentityRes>(entity);
        }

        public async Task<DriverLicenseRes?> GetMyDriverLicenseAsync(Guid userId)
        {
            var entity = await _driverService.GetByUserIdAsync(userId);
            if (entity == null)
                throw new NotFoundException(Message.UserMessage.DriverLicenseNotFound);

            return _mapper.Map<DriverLicenseRes>(entity);
        }

        public async Task<CitizenIdentityRes> UpdateCitizenIdentityAsync(Guid userId, UpdateCitizenIdentityReq req)
        {
            var entity = await _citizenIdentityRepository.GetByUserIdAsync(userId);
            if (entity == null)
                throw new NotFoundException(Message.UserMessage.CitizenIdentityNotFound);

            if (!string.IsNullOrWhiteSpace(req.Number) && req.Number != entity.Number)
                await VerifyUniqueNumberAsync.VerifyUniqueIdentityNumberAsync(req.Number, userId, _citizenIdentityRepository);
            if (!string.IsNullOrWhiteSpace(req.Number))
            {
                LisenceHelper.ValidateCitizenIdNumber(req.Number);
                entity.Number = req.Number.Trim();
            }
            if (!string.IsNullOrWhiteSpace(req.FullName)) entity.FullName = req.FullName.Trim();
            if (!string.IsNullOrWhiteSpace(req.Nationality)) entity.Nationality = req.Nationality.Trim();
            if (req.DateOfBirth.HasValue)
            {
                var age = VerifyUniqueNumberAsync.CalculateAge(req.DateOfBirth.Value);
                if (age < 21)
                    throw new BadRequestException(Message.UserMessage.InvalidUserAge);

                entity.DateOfBirth = req.DateOfBirth.Value;
            }
            if (req.ExpiresAt.HasValue) entity.ExpiresAt = req.ExpiresAt.Value;
            var existingLicense = await _driverLicenseRepository.GetByUserIdAsync(userId);
            if (existingLicense != null)
            {
                // Lấy giá trị final sau update
                var finalName = !string.IsNullOrWhiteSpace(req.FullName)
                    ? req.FullName.Trim()
                    : entity.FullName;

                DateTimeOffset finalDob = req.DateOfBirth ?? entity.DateOfBirth;

                LisenceHelper.EnsureMatch(
                    finalName,
                    finalDob,
                    existingLicense.FullName ?? "",
                    existingLicense.DateOfBirth
                );
            }
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await _citizenIdentityRepository.UpdateAsync(entity);
            return _mapper.Map<CitizenIdentityRes>(entity);
        }

        public async Task<DriverLicenseRes> UpdateDriverLicenseAsync(Guid userId, UpdateDriverLicenseReq req)
        {
            var entity = await _driverLicenseRepository.GetByUserIdAsync(userId);
            if (entity == null)
                throw new NotFoundException(Message.UserMessage.DriverLicenseNotFound);

            if (!string.IsNullOrWhiteSpace(req.Number) && req.Number != entity.Number)
                await VerifyUniqueNumberAsync.VerifyUniqueDriverLicenseNumberAsync(req.Number, userId, _driverLicenseRepository);

            if (req.Class.HasValue)
            {
                if (!Enum.IsDefined(typeof(LicenseClass), req.Class.Value))
                    throw new BadRequestException($"Invalid license class: {req.Class.Value}");

                entity.Class = (int)req.Class.Value;
            }

            if (!string.IsNullOrWhiteSpace(req.Number))
            {
                LisenceHelper.ValidateDriverLicenseNumber(req.Number);
                entity.Number = req.Number.Trim();
            }
            if (!string.IsNullOrWhiteSpace(req.FullName)) entity.FullName = req.FullName.Trim();
            if (!string.IsNullOrWhiteSpace(req.Nationality)) entity.Nationality = req.Nationality.Trim();
            if (req.Sex.HasValue) entity.Sex = req.Sex.Value;
            if (req.DateOfBirth.HasValue)
            {
                var age = VerifyUniqueNumberAsync.CalculateAge(req.DateOfBirth.Value);
                if (age < 21)
                    throw new BadRequestException(Message.UserMessage.InvalidUserAge);

                entity.DateOfBirth = req.DateOfBirth.Value;
            }
            if (req.ExpiresAt.HasValue) entity.ExpiresAt = req.ExpiresAt.Value;
            var existingCccd = await _citizenIdentityRepository.GetByUserIdAsync(userId);
            if (existingCccd != null)
            {
                var finalName = !string.IsNullOrWhiteSpace(req.FullName)
                    ? req.FullName.Trim()
                    : entity.FullName;

                DateTimeOffset finalDob = req.DateOfBirth ?? entity.DateOfBirth;

                LisenceHelper.EnsureMatch(
                    finalName,
                    finalDob,
                    existingCccd.FullName ?? "",
                    existingCccd.DateOfBirth
                );
            }
            entity.UpdatedAt = DateTimeOffset.UtcNow;

            await _driverLicenseRepository.UpdateAsync(entity);
            return _mapper.Map<DriverLicenseRes>(entity);
        }

        public async Task DeleteCitizenIdentityAsync(Guid userId)
        {
            var citizenIdentity = await _citizenIdentityRepository.GetByUserIdAsync(userId);
            if (citizenIdentity == null)
                throw new NotFoundException(Message.UserMessage.CitizenIdentityNotFound);

            if (!string.IsNullOrEmpty(citizenIdentity.FrontImagePublicId))
                await _photoService.DeletePhotoAsync(citizenIdentity.FrontImagePublicId);
            if (!string.IsNullOrEmpty(citizenIdentity.BackImagePublicId))
                await _photoService.DeletePhotoAsync(citizenIdentity.BackImagePublicId);
            citizenIdentity.DeletedAt = DateTimeOffset.UtcNow;
            await _mediaUow.SaveChangesAsync();
        }

        public async Task DeleteDriverLicenseAsync(Guid userId)
        {
            var driverLicense = await _driverLicenseRepository.GetByUserIdAsync(userId);
            if (driverLicense == null)
                throw new NotFoundException(Message.UserMessage.DriverLicenseNotFound);

            if (!string.IsNullOrEmpty(driverLicense.FrontImagePublicId))
                await _photoService.DeletePhotoAsync(driverLicense.FrontImagePublicId);
            if (!string.IsNullOrEmpty(driverLicense.BackImagePublicId))
                await _photoService.DeletePhotoAsync(driverLicense.BackImagePublicId);
            driverLicense.DeletedAt = DateTimeOffset.UtcNow;
            await _mediaUow.SaveChangesAsync();
        }
    }
}