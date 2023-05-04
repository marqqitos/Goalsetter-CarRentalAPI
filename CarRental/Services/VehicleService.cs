using CarRental.DAL;
using CarRental.DTOs;
using CarRental.Entities;
using CarRental.Exceptions;
using CarRental.Services.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Services
{
    public class VehicleService : IVehicleService
    {
        private readonly ILogger<VehicleService> _logger;
        private readonly RentalContext _context;
        private readonly IValidator<VehicleDTO> _vehicleDTOValidator;

        public VehicleService(ILogger<VehicleService> logger, RentalContext context, IValidator<VehicleDTO> vehicleDTOValidator)
        {
            _logger = logger;
            _context = context;
            _vehicleDTOValidator = vehicleDTOValidator;
        }

        public async Task<VehicleDTO> CreateVehicleAsync(VehicleDTO vehicleDTO)
        {
            _logger.LogInformation("Attempting to create vehicle: Make: {make}, Model: {model}, Chassis Number: {chassisNumber}, Price: ${pricePerDay}",
                    vehicleDTO.Make, vehicleDTO.Model, vehicleDTO.ChassisNumber, vehicleDTO.PricePerDay);
            
            _logger.LogDebug("Validating Vehicle {chassisNumber} to create", vehicleDTO.ChassisNumber);

            var validationResult = await _vehicleDTOValidator.ValidateAsync(vehicleDTO);

            if (!validationResult.IsValid)
                throw new ValidationException(validationResult.ToString());

            _logger.LogDebug("Vehicle {chassisNumber} is valid", vehicleDTO.ChassisNumber);

            _logger.LogDebug("Checking Vehicle with chassis number: {chassisNumber} has not been created", vehicleDTO.ChassisNumber);
            var existingVehicle = await _context.Vehicles.SingleOrDefaultAsync(v => v.ChassisNumber == vehicleDTO.ChassisNumber);

            if (existingVehicle is not null)
                throw new EntityExistsException("Vehicle is already created");

            _logger.LogDebug("Vehicle {chassisNumber} has not been created", vehicleDTO.ChassisNumber);

            _logger.LogInformation("Creating vehicle: Make: {make}, Model: {model}, Chassis Number: {chassisNumber}, Price: ${pricePerDay}",
                    vehicleDTO.Make, vehicleDTO.Model, vehicleDTO.ChassisNumber, vehicleDTO.PricePerDay);

            var vehicle = new Vehicle()
            {
                Make = vehicleDTO.Make,
                Model = vehicleDTO.Model,
                ChassisNumber = vehicleDTO.ChassisNumber,
                PricePerDay = vehicleDTO.PricePerDay,
                IsActive = true
            };

            _logger.LogDebug("Trying to insert Vehicle {chassisNumber} in DB", vehicleDTO.ChassisNumber);

            await _context.Vehicles.AddAsync(vehicle);
            await _context.SaveChangesAsync();

            _logger.LogDebug("Vehicle {chassisNumber} saved in DB", vehicleDTO.ChassisNumber);

            vehicleDTO.ID = vehicle.ID;

            _logger.LogInformation("Vehicle {vehicleId} with Chassis Number: {chassisNumber} created successfully",
                    vehicle.ID, vehicle.ChassisNumber);

            return vehicleDTO;
        }

        public async Task DeleteVehicleAsync(int id)
        {

            _logger.LogDebug("Checking if vehicle {vehicleId} exists", id);

            var existingVehicle = await _context.Vehicles.Include(v => v.Rentals)
                                                         .SingleOrDefaultAsync(v => v.ID == id);

            if (existingVehicle is null)
                throw new EntityNotFoundException("Vehicle does not exist");

            _logger.LogDebug("Vehicle {vehicleId} exists - checking is active", id);
            
            if (!existingVehicle.IsActive)
                return;
            
            _logger.LogDebug("Vehicle {vehicleId} exists and is active - checking is not rented", id);

            if (existingVehicle.IsRented())
                throw new VehicleRentedException("Vehicle is rented");

            _logger.LogInformation("Deleting vehicle {vehicleId}", id);

            existingVehicle.IsActive = false;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Vehicle {vehicleId} deleted successfully", id);
        }
    }
}
