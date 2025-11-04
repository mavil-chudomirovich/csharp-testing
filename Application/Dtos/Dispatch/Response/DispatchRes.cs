using Application.Constants;

namespace Application.Dtos.Dispatch.Response
{
    public class DispatchRes
    {
        public Guid Id { get; init; }
        public string? Description { get; init; }

        public Guid FromStationId { get; init; }
        public Guid ToStationId { get; init; }
        public string FromStationName { get; init; } = default!;
        public string ToStationName { get; init; } = default!;

        public DispatchRequestStatus Status { get; init; }

        public Guid RequestAdminId { get; init; }
        public string RequestAdminName { get; init; } = default!;

        public Guid? ApprovedAdminId { get; init; }
        public string? ApprovedAdminName { get; init; }

        public DateTimeOffset CreatedAt { get; init; }

        public IEnumerable<DispatchRequestStaffRes> DispatchRequestStaffs { get; init; }
            = new List<DispatchRequestStaffRes>();

        public IEnumerable<DispatchRequestVehicleRes> DispatchRequestVehicles { get; init; }
            = new List<DispatchRequestVehicleRes>();
    }
}
