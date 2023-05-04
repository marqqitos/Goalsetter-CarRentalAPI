using CarRental.DAL;
using CarRental.DTOs;
using CarRental.Entities;
using CarRental.Exceptions;
using CarRental.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Services
{
    public class RentalService : IRentalService
    {
        private readonly ILogger<RentalService> _logger;
        private readonly RentalContext _context;
        private readonly IValidator<RentalDTO> _rentalDTOValidator;

        public RentalService(ILogger<RentalService> logger, RentalContext context, IValidator<RentalDTO> rentalDTOValidator)
        {
            _logger = logger;
            _context = context;
            _rentalDTOValidator = rentalDTOValidator;
        }

        public async Task<RentalDTO> CreateRentalAsync(RentalDTO rentalDTO)
        {
            _logger.LogInformation("Attempting to create rental for period {startDate} - {endDate} for Client {clientId} with Vehicle {vehicleId}",
                    rentalDTO.StartDate, rentalDTO.EndDate, rentalDTO.Client.ID, rentalDTO.Vehicle.ID);

            _logger.LogDebug("Validating Rental to create");

            var validationResult = await _rentalDTOValidator.ValidateAsync(rentalDTO);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.ToString());

            _logger.LogDebug("Rental is valid");

            _logger.LogDebug("Checking client {id} of rental exists", rentalDTO.Client.ID);

            var existingClient = await _context.Clients.FindAsync(rentalDTO.Client.ID);
            ValidateClient(existingClient);

            _logger.LogDebug("Checking vehicle {id} of rental exists", rentalDTO.Vehicle.ID);
            
            var existingVehicle = await _context.Vehicles.Include(v => v.Rentals)
                                                         .SingleOrDefaultAsync(v => v.ID == rentalDTO.Vehicle.ID);

            ValidateVehicle(rentalDTO, existingVehicle);

            var rental = new Rental()
            {
                Client = existingClient,
                Vehicle = existingVehicle,
                StartDate = rentalDTO.StartDate.Date,
                EndDate = rentalDTO.EndDate.Date,
                IsActive = true
            };

            _logger.LogInformation("Creating rental for period {startDate} - {endDate} for Client {clientId} with Vehicle {vehicleId}",
                    rentalDTO.StartDate, rentalDTO.EndDate, rentalDTO.Client.ID, rentalDTO.Vehicle.ID);

            _logger.LogDebug("Calculating Rental Price");

            rental.CalculateRentalChargePrice();

            _logger.LogDebug("Rental price of {rentalPrice} calculated for vehicle price {vehiclePrice} and rental length {rentalLength}", 
                rental.RentalChargePrice, rental.Vehicle.PricePerDay, (rental.EndDate - rental.StartDate).TotalDays);

            _logger.LogDebug("Trying to insert rental in DB");

            await _context.Rentals.AddAsync(rental);
            await _context.SaveChangesAsync();

            _logger.LogDebug("Rental saved in DB");

            rentalDTO.ID = rental.ID;
            rentalDTO.RentalChargePrice = rental.RentalChargePrice;

            _logger.LogInformation("Rental {rentalId} for Client {clientId} with Vehicle {vehicleId} with a price of ${price} created succesfully",
                    rental.ID, rental.Client.ID, rental.Vehicle.ID, rental.RentalChargePrice);

            return rentalDTO;
        }

        public async Task CancelRentalAsync(int id)
        {
            _logger.LogDebug("Checking if rental {rentalId} exists", id);

            var existingRental = await _context.Rentals.FindAsync(id);

            if (existingRental is null)
                throw new EntityNotFoundException("Rental does not exist");

            _logger.LogDebug("Rental {rentalId} exists - Checking if is active", id);

            if (!existingRental.IsActive)
                return;

            _logger.LogInformation("Cancelling rental {rentalId}", existingRental.ID);
            
            existingRental.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Rental {rentalId} cancelled succesfully", existingRental.ID);
        }

        private void ValidateClient(Client existingClient)
        {
            if (existingClient is null)
            {
                _logger.LogError("Client does not exists");
                throw new EntityNotFoundException("Client does not exists");
            }

            _logger.LogDebug("Client {id} exists", existingClient.ID);

            _logger.LogDebug("Checking Client {id} is active", existingClient.ID);

            if (!existingClient.IsActive)
            {
                _logger.LogError("Client {id} deleted - not available for rental", existingClient.ID);
                throw new EntityNotAvailableException("Client deleted - not available for rental");
            }

            _logger.LogDebug("Client is available");
        }

        private void ValidateVehicle(RentalDTO rental, Vehicle existingVehicle)
        {
            if (existingVehicle is null)
            {
                _logger.LogError("Vehicle does not exists");
                throw new EntityNotFoundException("Vehicle does not exists");
            }

            _logger.LogDebug("Vehicle {id} exists", rental.Vehicle.ID);

            _logger.LogDebug("Checking Vehicle {id} is active", rental.Vehicle.ID);

            if (!existingVehicle.IsActive)
            {
                _logger.LogError("Vehicle {id} deleted - not available for rental", existingVehicle.ID);
                throw new EntityNotAvailableException("Vehicle deleted - not available for rental");
            }

            _logger.LogDebug("Vehicle {id} is active - Checking is available for rental", existingVehicle.ID);

            if (!existingVehicle.IsAvailableForRental(rental.StartDate, rental.EndDate))
                throw new VehicleRentedException("Vehicle is already rented");

            _logger.LogDebug("Vehice is available");
        }
    }
}
