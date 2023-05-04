using CarRental.DTOs;

namespace CarRental.Services.Interfaces
{
    public interface IVehicleService
    {
        public Task<VehicleDTO> CreateVehicleAsync(VehicleDTO vehicleDTO);
        public Task DeleteVehicleAsync(int id);
    }
}
