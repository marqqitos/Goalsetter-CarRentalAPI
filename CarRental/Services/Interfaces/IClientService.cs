using CarRental.DTOs;

namespace CarRental.Services.Interfaces
{
    public interface IClientService
    {
        public Task<ClientDTO> CreateClientAsync(ClientDTO clientDTO);
        public Task DeleteClientAsync(int id);
    }
}
