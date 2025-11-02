using Application.Abstractions;
using Application.AppExceptions;
using Application.Constants;
using Application.Helpers;
using Application.Repositories;
using Domain.Entities;

namespace Application
{
    public class CitizenIdentityService : ICitizenIdentityService
    {
        private readonly IGeminiService _geminiService;
        private readonly ICitizenIdentityRepository _citizenRepo;
        private readonly IPhotoService _photoService;

        public CitizenIdentityService(
            IGeminiService geminiService,
            ICitizenIdentityRepository citizenRepo,
            IPhotoService photoService)
        {
            _geminiService = geminiService;
            _citizenRepo = citizenRepo;
            _photoService = photoService;
        }

        public async Task<CitizenIdentity> AddAsync(CitizenIdentity identity)
        {
            identity.CreatedAt = DateTimeOffset.UtcNow;
            identity.UpdatedAt = DateTimeOffset.UtcNow;
            await _citizenRepo.AddAsync(identity);
            return identity;
        }

        public async Task<CitizenIdentity?> GetByIdAsync(Guid id)
            => await _citizenRepo.GetByIdAsync(id);

        public async Task<CitizenIdentity?> GetByIdentityNumberAsync(string identityNumber)
        {
            var citizenIdentity = await _citizenRepo.GetByIdNumberAsync(identityNumber);
            if (citizenIdentity == null)
            {
                throw new NotFoundException(Message.UserMessage.CitizenIdentityNotFound);
            }
            return citizenIdentity;
        }

        public async Task<CitizenIdentity?> GetByUserId(Guid userId)
            => await _citizenRepo.GetByUserIdAsync(userId);

        public async Task<CitizenIdentity?> ProcessCitizenIdentityAsync(Guid userId,
            string frontImageUrl, string frontPublicId, string backImageUrl, string backPublicId)
        {
            var dto = await _geminiService.ExtractCitizenIdAsync(frontImageUrl);
            if (dto == null)
                throw new BusinessException(Message.UserMessage.InvalidCitizenIdData);

            DateTimeOffset.TryParse(dto.DateOfBirth, out var dob);
            DateTimeOffset.TryParse(dto.ExpiresAt, out var exp);
            await VerifyUniqueNumberAsync.VerifyUniqueIdentityNumberAsync(dto.IdNumber ?? string.Empty, userId, _citizenRepo);
            var entity = new CitizenIdentity
            {
                UserId = userId,
                Number = dto.IdNumber?.Trim() ?? string.Empty,
                FullName = NormalizeName(dto.FullName),
                Nationality = dto.Nationality?.Trim() ?? string.Empty,
                Sex = ParseSex(dto.Sex),
                DateOfBirth = dob == default ? DateTimeOffset.MinValue : dob,
                ExpiresAt = exp == default ? DateTimeOffset.MinValue : exp,
                FrontImageUrl = frontImageUrl,
                FrontImagePublicId = frontPublicId,
                BackImageUrl = backImageUrl,
                BackImagePublicId=backPublicId,
                UpdatedAt = DateTimeOffset.UtcNow
            };

            var existing = await _citizenRepo.GetByUserIdAsync(userId);
            if (existing != null)
            {
                entity.Id = existing.Id;
                await _citizenRepo.UpdateAsync(entity);
            }
            else
            {
                entity.CreatedAt = DateTimeOffset.UtcNow;
                await _citizenRepo.AddAsync(entity);
            }

            return entity;
        }

        public async Task<bool> RemoveAsync(Guid userId, string publicId)
        {
            var existing = await _citizenRepo.GetByUserIdAsync(userId);
            if (existing == null)
                throw new NotFoundException(Message.UserMessage.NotFound);

            await _citizenRepo.DeleteAsync(existing.Id);
            await _photoService.DeletePhotoAsync(publicId);
            return true;
        }

        public async Task<CitizenIdentity?> UpdateAsync(CitizenIdentity identity)
        {
            var existing = await _citizenRepo.GetByIdAsync(identity.Id);
            if (existing == null)
                throw new NotFoundException(Message.UserMessage.NotFound);

            identity.UpdatedAt = DateTimeOffset.UtcNow;
            await _citizenRepo.UpdateAsync(identity);
            return identity;
        }

        private static int ParseSex(string? sexStr)
        {
            if (string.IsNullOrWhiteSpace(sexStr)) return 0;
            var s = sexStr.Trim().ToLower();
            if (s.Contains("nữ") || s.Contains("female") || s == "f") return 1;
            return 0;
        }

        private static string NormalizeName(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return string.Empty;
            var parts = fullName.Trim().ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < parts.Length; i++)
                parts[i] = char.ToUpper(parts[i][0]) + parts[i].Substring(1);
            return string.Join(' ', parts);
        }

        public async Task<string> VerifyDocumentTypeAsync(string imageUrl)
        {
            var type = await _geminiService.DetectDocumentTypeAsync(imageUrl);
            return type ?? "Unknown";
        }
    }
}