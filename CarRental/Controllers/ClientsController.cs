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
    public class ClientsController : ControllerBase
    {
        private readonly ILogger<ClientsController> _logger;
        private readonly IClientService _clientService;

        public ClientsController(ILogger<ClientsController> logger, IClientService clientService)
        {
            _logger = logger;
            _clientService = clientService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateClient(ClientDTO clientDTO)
        {
            try
            {
                var client = await _clientService.CreateClientAsync(clientDTO);

                return Ok(client);
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, "Error creating client {email} - Pre-validation failed", clientDTO.Email);

                return BadRequest(ex.Message);
            }
            catch (EntityExistsException ex)
            {
                _logger.LogError(ex, "Error creating client {email}, client already exists", clientDTO.Email);

                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating client {email}", clientDTO.Email);

                return StatusCode(500);
            }
        }

        [HttpDelete("{clientId}")]
        public async Task<IActionResult> DeleteClient(int clientId)
        {
            try
            {
                await _clientService.DeleteClientAsync(clientId);

                return NoContent();
            }
            catch (EntityNotFoundException ex)
            {
                _logger.LogError(ex, "Error deleting client {clientId}, client does not exist", clientId);
                
                return BadRequest(ex.Message);
            }
            catch (ClientHasActiveRentalException ex)
            {
                _logger.LogError(ex, "Error deleting client {clientId}, client has active rentals", clientId);
                
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting client {clientId}", clientId);

                return StatusCode(500);
            }
        }
    }
}
