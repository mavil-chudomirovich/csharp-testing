using Application.Dtos.CitizenIdentity.Request;
using Application.Dtos.DriverLicense.Request;
using Domain.Entities;

namespace Application.Abstractions
{
    public interface IGeminiService
    {
        Task<CreateCitizenIdentityReq?> ExtractCitizenIdAsync(string imageUrl);

        Task<CreateDriverLicenseReq?> ExtractDriverLicenseAsync(string imageUrl);
        Task<string?> DetectDocumentTypeAsync(string imageUrl);
    }
}