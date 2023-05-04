using System.Text.Json.Serialization;

namespace CarRental.DTOs
{
    public class ClientDTO
    {
        public int ID { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }
    }
}
