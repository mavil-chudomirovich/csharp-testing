using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Statistic.Responses
{
    public sealed class RevenueByMonthRes
    {
        public string MonthName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
    }
}
