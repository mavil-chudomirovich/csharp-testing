using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Dtos.Dispatch.Response;

namespace Application.Dtos.Dispatch.Request
{
    public sealed class ConfirmDispatchReq
    {
        public int Status { get; set; }
        public Guid? FromStationId { get; set; }
        public DispatchDescriptionDto? FinalDescription { get; set; }
    }
}