namespace CarRental.Entities
{
    public class Client
    {
        public int ID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set;}
        public string Email { get; set; }
        public bool IsActive { get; set; }

        public ICollection<Rental> Rentals { get; set; }

        public bool HasActiveRentals()
            => Rentals.Any(r => r.ClientID == ID && r.IsActive && r.EndDate >= DateTime.Today);

    }
}
