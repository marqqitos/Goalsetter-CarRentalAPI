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
    public class RentalsController : ControllerBase
    {
        private readonly ILogger<RentalsController> _logger;
        private readonly IRentalService _rentalService;

        public RentalsController(ILogger<RentalsController> logger, IRentalService rentalService)
        {
            _logger = logger;
            _rentalService = rentalService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRental(RentalDTO rentalDTO)
        {
            try
            {
                var rental = await _rentalService.CreateRentalAsync(rentalDTO);
                
                return Ok(rental);
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, "Error creating rental - Prevalidation failed");

                return BadRequest(ex.Message);
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogError(ex, "Error creating rental - Entity not found");

                return BadRequest(ex.Message);
            }
            catch (EntityNotAvailableException ex)
            {
                _logger.LogError(ex, "Error creating rental - Entity not available");

                return BadRequest(ex.Message);
            }
            catch (VehicleRentedException ex)
            {
                _logger.LogError(ex, "Error creating rental - vehicle {vehicleId} with chassis number {chassis number} is already rented",
                    rentalDTO.Vehicle.ID, rentalDTO.Vehicle.ChassisNumber);

                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating rental");

                return StatusCode(500);
            }
        }

        [HttpPut("/{rentalId}/cancellation")]
        public async Task<IActionResult> CancelRental(int rentalId)
        {
            try
            {
                await _rentalService.CancelRentalAsync(rentalId);

                return NoContent();
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogError(ex, "Error cancelling rental {rentalId} - Rental not found", rentalId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting rental");

                return StatusCode(500);
            }
        }
    }
}
