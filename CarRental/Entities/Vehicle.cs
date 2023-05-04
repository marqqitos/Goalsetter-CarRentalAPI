namespace CarRental.Entities
{
    public class Vehicle
    {
        public int ID { get; set; }
        public string ChassisNumber { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public decimal PricePerDay { get; set; }
        public bool IsActive { get; set; }

        public ICollection<Rental> Rentals { get; set; }
        
        public bool IsRented()
            => Rentals.Any(r => r.VehicleID == ID && r.IsActive && r.EndDate >= DateTime.Today);

        public bool IsAvailableForRental(DateTime startDate, DateTime endDate)
            => !Rentals.Any(r => r.IsActive &&
                                 (
                                    (r.StartDate <= startDate && startDate <= r.EndDate) ||
                                    (r.StartDate <= endDate && endDate <= r.EndDate) ||
                                    (startDate <= r.StartDate && r.StartDate <= endDate) ||
                                    (startDate <= r.EndDate && r.EndDate <= endDate)
                                 )
                            );
    }
}
