using CarRental.DTOs;
using CarRental.Entities;
using CarRental.Exceptions;
using CarRental.Services.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CarRental.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VehiclesController : ControllerBase
    {
        private readonly ILogger<VehiclesController> _logger;
        private readonly IVehicleService _vehicleService;

        public VehiclesController(ILogger<VehiclesController> logger, IVehicleService vehicleService)
        {
            _logger = logger;
            _vehicleService = vehicleService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateVehicle(VehicleDTO vehicleDTO)
        {
            try
            {
                var vehicle = await _vehicleService.CreateVehicleAsync(vehicleDTO);

                return Ok(vehicle);
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, "Error creating vehicle {chassisNumber} - Pre-validation failed", vehicleDTO.ChassisNumber);

                return BadRequest(ex.Message);
            }
            catch (EntityExistsException ex)
            {
                _logger.LogError(ex, "Error creating vehicle {chassisNumber} - Vehicle already exists", vehicleDTO.ChassisNumber);

                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating vehicle {chassisNumber}", vehicleDTO.ChassisNumber);

                return StatusCode(500);
            }

        }

        [HttpDelete("{vehicleId}")]
        public async Task<IActionResult> DeleteVehicle(int vehicleId)
        {
            try
            {
                await _vehicleService.DeleteVehicleAsync(vehicleId);

                return NoContent();
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogError(ex, "Error deleting vehicle {vehicleId} - Vehicle not found", vehicleId);
                
                return BadRequest(ex.Message);
            }
            catch (VehicleRentedException ex)
            {
                _logger.LogError(ex, "Error deleting vehicle {vehicleId} - Vehicle is rented", vehicleId);
                
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting vehicle {vehicleId}", vehicleId);

                return StatusCode(500);
            }
        }

    }
}
