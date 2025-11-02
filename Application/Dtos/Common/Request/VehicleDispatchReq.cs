using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Common.Request
{
    public class VehicleDispatchReq
    {
        public Guid ModelId { get; set; }
        public int NumberOfVehicle { get; set; }
    }
}
