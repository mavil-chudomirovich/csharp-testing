using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.RentalContract.Respone
{
    public class BestRentedModel
    {
        public Guid ModelId { get; set; }
        public string ModelName { get; set; } = string.Empty;
        public int RentedCount { get; set; }
    }
}
