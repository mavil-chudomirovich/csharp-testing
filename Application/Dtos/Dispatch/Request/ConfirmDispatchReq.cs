using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Dispatch.Request
{
    public sealed class ConfirmDispatchReq
    {
        public int Status { get; set; }
        public Guid? FromStationId { get; set; }
    }
}