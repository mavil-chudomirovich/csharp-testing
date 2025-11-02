using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Dtos.Vehicle.Respone
{
    public class VehicleModelsForStatisticRes
    {
        public Guid ModelId { get; set; }
        public string ModelName { get; set; } = string.Empty;
        public int NumberOfAvailable { get; set; }
        public int NumberOfRented { get; set; }
        public int NumberOfMaintenance { get; set; }

        public VehicleModelsForStatisticRes(Guid ModelId, string ModelName, int NumberOfAvailable, int NumberOfRented, int NumberOfMaintenance) 
        {
            this.ModelId = ModelId;
            this.ModelName = ModelName;
            this.NumberOfAvailable = NumberOfAvailable;
            this.NumberOfMaintenance = NumberOfMaintenance;
            this.NumberOfRented = NumberOfRented;
        }

    }
}
