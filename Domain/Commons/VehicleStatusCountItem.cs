using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Commons
{
    public class VehicleStatusCountItem
    {
        public int Status { get; set; }
        public int NumberOfVehicle { get; set; }
        public VehicleStatusCountItem(int Status, int NumberOfVehicle) 
        { 
            this.Status = Status;
            this.NumberOfVehicle = NumberOfVehicle;

        }
    }
}
