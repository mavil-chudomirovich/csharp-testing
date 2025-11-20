using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Ticket.Request
{
    public class CreateContactReq
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
    }
}