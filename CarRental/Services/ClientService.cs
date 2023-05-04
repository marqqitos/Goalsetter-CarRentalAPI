using CarRental.DAL;
using CarRental.DTOs;
using CarRental.Entities;
using CarRental.Exceptions;
using CarRental.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Services
{
    public class ClientService : IClientService
    {
        private readonly ILogger<ClientService> _logger;
        private readonly RentalContext _context;
        private readonly IValidator<ClientDTO> _clientDTOValidator;

        public ClientService(ILogger<ClientService> logger, RentalContext context, IValidator<ClientDTO> clientDTOValidator)
        {
            _logger = logger;
            _context = context;
            _clientDTOValidator = clientDTOValidator;
        }

        public async Task<ClientDTO> CreateClientAsync(ClientDTO clientDTO)
        {
            _logger.LogInformation("Attempting to create client {email}", clientDTO.Email);
            _logger.LogDebug("Validating Client {email} to create", clientDTO.Email);

            var validationResult = await _clientDTOValidator.ValidateAsync(clientDTO);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.ToString());

            _logger.LogDebug("Client {email} is valid", clientDTO.Email);

            _logger.LogDebug("Checking Client {email} has not been created", clientDTO.Email);
            var existingClient = await _context.Clients.SingleOrDefaultAsync(c => c.Email == clientDTO.Email);

            if (existingClient is not null)
                throw new EntityExistsException("Client is already created");

            _logger.LogDebug("Client {email} has not been created", clientDTO.Email);

            _logger.LogInformation("Creating client: Name: {name}, LastName: {lastName}, Email: {email}", clientDTO.FirstName, clientDTO.LastName, clientDTO.Email);

            var client = new Client
            {
                FirstName = clientDTO.FirstName,
                LastName = clientDTO.LastName,
                Email = clientDTO.Email,
                IsActive = true
            };

            _logger.LogDebug("Trying to insert Client {email} in DB", clientDTO.Email);

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            _logger.LogDebug("Client {email} saved in DB", clientDTO.Email);

            clientDTO.ID = client.ID;

            _logger.LogInformation("Client id: {id} - email: {email} created succesfully", client.ID, client.Email);

            return clientDTO;
        }

        public async Task DeleteClientAsync(int id)
        {
            _logger.LogInformation("Attempting to delete client {clientId}", id);

            _logger.LogDebug("Checking if client {clientId} exists", id);

            var existingClient = await _context.Clients.Include(c => c.Rentals)
                                                       .SingleOrDefaultAsync(c => c.ID == id);

            if (existingClient is null)
                throw new EntityNotFoundException("Client does not exists");

            _logger.LogDebug("Client {clientId} exists - checking is active", id);

            if (!existingClient.IsActive)
            {
                _logger.LogInformation("Client is not active");
                return;
            }

            _logger.LogDebug("Checking Client {clientId} does not have any ongoing rental", id);
            if (existingClient.HasActiveRentals())
                throw new ClientHasActiveRentalException("Client has active rentals");

            _logger.LogInformation("Deleting client {clientId}", id);

            existingClient.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Client {clientId} deleted succesfully", id);
        }
    }
}
