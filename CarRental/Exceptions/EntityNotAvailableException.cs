namespace CarRental.Exceptions
{
    public class EntityNotAvailableException : Exception
    {
        public EntityNotAvailableException(string message) : base(message) { }
    }
}
