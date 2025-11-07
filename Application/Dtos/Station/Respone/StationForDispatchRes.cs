using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos.Dispatch.Response;

namespace Application.Dtos.Station.Respone
{
    public class StationForDispatchRes
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public DispatchDescriptionDto AvailableDescription { get; set; } = null!;
    }
}