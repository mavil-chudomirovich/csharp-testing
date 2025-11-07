using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Statistic.Responses
{
    public class ContractByMonthRes
    {
        public string MonthName { get; set; } = string.Empty;
        public decimal TotalContract { get; set; }
    }
}
