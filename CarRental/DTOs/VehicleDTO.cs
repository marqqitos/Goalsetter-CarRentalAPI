using System.Text.Json.Serialization;

namespace CarRental.DTOs
{
    public class VehicleDTO
    {
        public int ID { get; set; }

        public string ChassisNumber { get; set; }

        public string Make { get; set; }

        public string Model { get; set; }

        public decimal PricePerDay { get; set; }
    }
}
