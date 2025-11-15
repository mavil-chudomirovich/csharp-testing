using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Dtos.Statistic.Responses
{
    public class TotalStatisticRes<T> where T : INumber<T>
    {
        public required T TotalThisMonth { get; set; }
        public required T TotalLastMonth { get; set; }
        public decimal ChangeRate { get; set; }
    }
}