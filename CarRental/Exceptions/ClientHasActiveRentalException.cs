namespace CarRental.Exceptions
{
    public class ClientHasActiveRentalException : Exception
    {
        public ClientHasActiveRentalException(string message) : base(message) { }
    }
}
