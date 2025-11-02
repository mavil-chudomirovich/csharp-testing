using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Dispatch.Request
{
    public sealed class UpdateDispatchReq
    {
        public int Status { get; set; }
        public Guid[] StaffIds { get; set; } = [];
        public Guid[] VehicleIds { get; set; } = [];
        public string? Description { get; set; }
    }
}
