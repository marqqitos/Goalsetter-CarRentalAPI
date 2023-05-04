using AutoFixture;
using CarRental.DTOs;
using CarRental.Exceptions;
using CarRental.Services.Interfaces;
using CarRental.Services;
using FluentAssertions;
using CarRental.DAL;
using Moq;
using CarRental.Entities;
using Moq.EntityFrameworkCore;
using FluentValidation;
using Microsoft.Extensions.Logging;
using FluentValidation.Results;

namespace CarRentalTests.Services
{
    [TestFixture]
    public class ClientServiceTests
    {
        private IClientService _sut;
        private Fixture _fixture;

        private Mock<RentalContext> _mockDBContext;
        private Mock<IValidator<ClientDTO>> _mockValidator;
        private Mock<ILogger<ClientService>> _mockLogger;

        [SetUp]
        public void Setup()
        {
            _mockDBContext = new Mock<RentalContext>();
            _mockDBContext.Setup(mdbc => mdbc.Clients).ReturnsDbSet(new List<Client>());
            _mockDBContext.Setup(mdbc => mdbc.Rentals).ReturnsDbSet(new List<Rental>());

            _mockValidator = new Mock<IValidator<ClientDTO>>();
            _mockLogger = new Mock<ILogger<ClientService>>();

            _sut = new ClientService(_mockLogger.Object, _mockDBContext.Object, _mockValidator.Object);
            _fixture = new Fixture();
        }

        private Client GetClientFromDTO(ClientDTO dto) 
            => new Client() 
            {
                ID = dto.ID,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                IsActive = true
            };

        [Test]
        [Category("CreateClientAsync")]
        public async Task CreateClientAsync_WithValidClient_ReturnsCreatedClient()
        {
            // Arrange
            var clientDTO = _fixture.Create<ClientDTO>();

            _mockValidator.Setup(mv => mv.ValidateAsync(It.IsAny<ClientDTO>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult());

            // Act
            var result = await _sut.CreateClientAsync(clientDTO);

            // Assert
            result.ID.Should().NotBe(null);
            result.FirstName.Should().Be(clientDTO.FirstName);
            result.LastName.Should().Be(clientDTO.LastName);
            result.Email.Should().Be(clientDTO.Email);
        }

        [Test]
        [Category("CreateClientAsync")]
        public async Task CreateClientAsync_WithInvalidInputData_ThrowsValidationException()
        {
            // Arrange
            var clientDTO = new ClientDTO()
            {
                FirstName = null,
                Email = null,
                LastName = null
            };

            _mockValidator.Setup(mv => mv.ValidateAsync(It.IsAny<ClientDTO>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult(new List<ValidationFailure>() { new ValidationFailure() }));

            _mockValidator.Setup(mv => mv.ValidateAsync(It.IsAny<ClientDTO>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult());

            // Act
            var result = await _sut.CreateClientAsync(clientDTO);

            // Assert
            result.ID.Should().NotBe(null);
            result.FirstName.Should().Be(clientDTO.FirstName);
            result.LastName.Should().Be(clientDTO.LastName);
            result.Email.Should().Be(clientDTO.Email);
        }

        [Test]
        [Category("CreateClientAsync")]
        public async Task CreateClientAsync_WhenClientAlreadyExists_ThrowsEntityExistsException()
        {
            // Arrange
            var clientDTO = _fixture.Create<ClientDTO>();
            var client = GetClientFromDTO(clientDTO);

            _mockDBContext.Setup(mdbc => mdbc.Clients)
                          .ReturnsDbSet(new List<Client>() { client });

            _mockValidator.Setup(mv => mv.ValidateAsync(It.IsAny<ClientDTO>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult());

            // Act
            Func<Task> action = async () => await _sut.CreateClientAsync(clientDTO);

            // Assert
            await action.Should().ThrowAsync<EntityExistsException>();
        }

        [Test]
        [Category("DeleteClientAsync")]
        public async Task DeleteClientAsync_WhenClientExistsAndIsActive_DeletesClient()
        {
            // Arrange
            var clientDTO = _fixture.Create<ClientDTO>();
            var client = GetClientFromDTO(clientDTO);
            client.Rentals = new List<Rental>();

            _mockDBContext.Setup(mdbc => mdbc.Clients)
                          .ReturnsDbSet(new List<Client>() { client });

            // Act
            await _sut.DeleteClientAsync(clientDTO.ID);

            // Assert
            _mockDBContext.Verify(mdbc => mdbc.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            client.IsActive.Should().BeFalse();
        }

        [Test]
        [Category("DeleteClientAsync")]
        public async Task DeleteClientAsync_WhenClientExistsAndIsNotActive_DoNothings()
        {
            // Arrange
            var clientDTO = _fixture.Create<ClientDTO>();
            var client = GetClientFromDTO(clientDTO);
            client.IsActive = false;

            _mockDBContext.Setup(mdbc => mdbc.Clients)
                          .ReturnsDbSet(new List<Client>() { client });

            // Act
            await _sut.DeleteClientAsync(clientDTO.ID);

            // Assert
            _mockDBContext.Verify(mdbc => mdbc.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        [Category("DeleteClientAsync")]
        public async Task DeleteClientAsync_WhenClientDoesNotExists_ThrowsEntityNotFoundException()
        {
            // Arrange
            var clientDTO = _fixture.Create<ClientDTO>();

            // Act
            Func<Task> act = async () => await _sut.DeleteClientAsync(clientDTO.ID);

            // Assert
            await act.Should().ThrowAsync<EntityNotFoundException>();
        }

        [Test]
        [Category("DeleteClientAsync")]
        public async Task DeleteClientAsync_WhenClientHasActiveRentals_ThrowsClientHasActiveRentalException()
        {
            // Arrange
            var client = _fixture.Build<Client>()
                                  .Without(v => v.Rentals)
                                  .Create();

            var rental = _fixture.Build<Rental>()
                                 .With(r => r.ClientID, client.ID)
                                 .Without(r => r.Client)
                                 .Without(r => r.Vehicle)
                                 .With(r => r.StartDate, DateTime.Now.AddDays(2))
                                 .With(r => r.EndDate, DateTime.Now.AddDays(5))
                                 .With(r => r.IsActive, true)
                                 .Create();

            client.Rentals = new List<Rental> { rental };

            _mockDBContext.Setup(mdbc => mdbc.Clients)
                          .ReturnsDbSet(new List<Client>() { client });

            // Act
            Func<Task> action = async () => await _sut.DeleteClientAsync(client.ID);

            // Assert
            await action.Should().ThrowAsync<ClientHasActiveRentalException>();
        }
    }
}
