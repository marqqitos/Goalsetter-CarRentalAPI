using System.Text.Json.Serialization;

namespace CarRental.DTOs
{
    public class RentalDTO
    {
        public int ID { get; set; }

        public VehicleDTO Vehicle { get; set; }

        public ClientDTO Client { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public decimal RentalChargePrice { get; set; }

    }
}
