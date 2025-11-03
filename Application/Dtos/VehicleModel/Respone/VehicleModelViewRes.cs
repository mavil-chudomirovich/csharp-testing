using Application.Dtos.Brand.Respone;
using Application.Dtos.VehicleSegment.Respone;
using Domain.Entities;

namespace Application.Dtos.VehicleModel.Respone
{
    public class VehicleModelViewRes
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public decimal CostPerDay { get; set; }
        public decimal DepositFee { get; set; }
        public decimal ReservationFee { get; set; }
        public int SeatingCapacity { get; set; }
        public int NumberOfAirbags { get; set; }
        public decimal MotorPower { get; set; }
        public decimal BatteryCapacity { get; set; }
        public decimal EcoRangeKm { get; set; }
        public decimal SportRangeKm { get; set; }

        public string? ImageUrl { get; set; }
        public IEnumerable<string> ImageUrls { get; set; } = Enumerable.Empty<string>();

        public BrandViewRes Brand { get; set; } = null!;
        public VehicleSegmentViewRes Segment { get; set; } = null!;
        public int AvailableVehicleCount { get; set; }
    }
}