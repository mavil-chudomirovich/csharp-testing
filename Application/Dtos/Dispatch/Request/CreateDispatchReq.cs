using Application.Constants;
using Application.Dtos.Common.Request;

namespace Application.Dtos.Dispatch.Request
{
    public sealed class CreateDispatchReq
    {
        public Guid FromStationId { get; set; }
        public VehicleDispatchReq[] Vehicles { get; set; } = [];
        public int? NumberOfStaff { get; set; }

    }
} 