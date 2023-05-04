using CarRental.DAL;
using CarRental.DTOs;

namespace CarRental.Services.Interfaces
{
    public interface IRentalService
    {
        public Task<RentalDTO> CreateRentalAsync(RentalDTO rentalDTO);
        public Task CancelRentalAsync(int id);
    }
}
