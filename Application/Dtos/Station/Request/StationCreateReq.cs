using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Station.Request
{
    public class StationCreateReq
    {
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
    }
}
