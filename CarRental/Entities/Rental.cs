namespace CarRental.Entities
{
    public class Rental
    {
        public int ID { get; set; }
        public int VehicleID { get; set; }
        public Vehicle Vehicle { get; set; }
        public int ClientID { get; set; }  
        public Client Client { get; set; }  
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal RentalChargePrice { get; set; }
        public bool IsActive { get; set; }

        public void CalculateRentalChargePrice()
            => RentalChargePrice = Convert.ToDecimal((EndDate - StartDate).TotalDays) * Vehicle.PricePerDay;
    }
}
