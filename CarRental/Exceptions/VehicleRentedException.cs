namespace CarRental.Exceptions
{
    public class VehicleRentedException : Exception
    {
        public VehicleRentedException(string message) : base(message) { }
    }
}
