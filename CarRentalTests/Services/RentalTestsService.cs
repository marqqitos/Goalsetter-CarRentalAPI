using AutoFixture;
using CarRental.DAL;
using CarRental.DTOs;
using CarRental.Entities;
using CarRental.Exceptions;
using CarRental.Services;
using CarRental.Services.Interfaces;
using Castle.Core.Logging;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;

namespace CarRentalTests.Services
{
    [TestFixture]
    public class RentalTestsService
    {
        private IRentalService _sut;
        private Fixture _fixture;

        private Mock<RentalContext> _mockDBContext;
        private Mock<IValidator<RentalDTO>> _mockValidator;
        private Mock<ILogger<RentalService>> _mockLogger;

        [SetUp]
        public void SetUp()
        {
            _mockDBContext = new Mock<RentalContext>();
            _mockDBContext.Setup(mdbc => mdbc.Vehicles).ReturnsDbSet(new List<Vehicle>());
            _mockDBContext.Setup(mdbc => mdbc.Rentals).ReturnsDbSet(new List<Rental>());
            _mockDBContext.Setup(mdbc => mdbc.Clients).ReturnsDbSet(new List<Client>());

            _mockValidator = new Mock<IValidator<RentalDTO>>();
            _mockLogger = new Mock<ILogger<RentalService>>();

            _sut = new RentalService(_mockLogger.Object, _mockDBContext.Object, _mockValidator.Object);
            _fixture = new Fixture();
        }

        public Client GetClientFromDTO(ClientDTO dto)
            => new Client()
            {
                ID = dto.ID,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                IsActive = true
            };

        public Vehicle GetVehicleFromDTO(VehicleDTO dto)
            => new Vehicle()
            {
                ID = dto.ID,
                ChassisNumber = dto.ChassisNumber,
                Make = dto.Make,
                Model = dto.Model,
                PricePerDay = dto.PricePerDay,
                IsActive = true
            };

        [Test]
        [Category("CreateRentalAsync")]
        public async Task CreateRentalAsync_WhenRentalIsValidAndVehicleHas10USDPerDayPriceAndRentalIs10DaysLong_ReturnsCreatedRentalWith100TotalPrice()
        {
            //Arrange
            var vehicleDTO = _fixture.Build<VehicleDTO>()
                                     .With(v => v.PricePerDay, 10)
                                     .Create();

            var vehicle = GetVehicleFromDTO(vehicleDTO);
            vehicle.Rentals = new List<Rental>();

            var clientDTO = _fixture.Create<ClientDTO>();

            var client = GetClientFromDTO(clientDTO);

            var rentalDTO = _fixture.Build<RentalDTO>()
                                    .With(r => r.Vehicle, vehicleDTO)
                                    .With(r => r.Client, clientDTO)
                                    .With(r => r.StartDate, DateTime.Now.AddDays(3))
                                    .With(r => r.EndDate, DateTime.Now.AddDays(13))
                                    .Create();

            _mockDBContext.Setup(mdbc => mdbc.Vehicles).ReturnsDbSet(new List<Vehicle> { vehicle });
            _mockDBContext.Setup(mdbc => mdbc.Clients.FindAsync(It.IsAny<int>())).ReturnsAsync(client);

            _mockValidator.Setup(mv => mv.ValidateAsync(It.IsAny<RentalDTO>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult());

            //Act
            var result = await _sut.CreateRentalAsync(rentalDTO);

            //Assert
            result.ID.Should().NotBe(null);
            result.Vehicle.Should().BeEquivalentTo(rentalDTO.Vehicle);
            result.Client.Should().BeEquivalentTo(rentalDTO.Client);
            result.RentalChargePrice.Should().BeApproximately(100M, 0.001M);
        }

        [Test]
        [Category("CreateRentalAsync")]
        public async Task CreateRentalAsync_WhenRentalIsInvalid_ThrowsValidationException()
        {
            // Arrange
            var rentalDTO = _fixture.Build<RentalDTO>()
                                     .With(v => v.StartDate, DateTime.Today.AddDays(-1))
                                     .Create();

            _mockValidator.Setup(mv => mv.ValidateAsync(It.IsAny<RentalDTO>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult(new List<ValidationFailure>() { new ValidationFailure() }));

            // Act
            Func<Task> action = async () => await _sut.CreateRentalAsync(rentalDTO);

            // Assert
            await action.Should().ThrowAsync<ValidationException>();
        }

        [Test]
        [Category("CreateRentalAsync")]
        public async Task CreateRentalAsync_WhenClientDoesNotExist_ThrowsEntityNotFoundException()
        {
            //Arrange
            var rentalDTO = _fixture.Build<RentalDTO>()
                                 .With(r => r.StartDate, DateTime.Now.AddDays(3))
                                 .With(r => r.EndDate, DateTime.Now.AddDays(5))
                                 .Create();

            _mockValidator.Setup(mv => mv.ValidateAsync(It.IsAny<RentalDTO>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult());

            _mockDBContext.Setup(mdbc => mdbc.Clients.FindAsync(It.IsAny<int>())).ReturnsAsync((Client)null);

            //Act
            Func<Task> act = async () => await _sut.CreateRentalAsync(rentalDTO);

            //Assert
            await act.Should().ThrowAsync<EntityNotFoundException>();
        }

        [Test]
        [Category("CreateRentalAsync")]
        public async Task CreateRentalAsync_WhenClientIsNoLongerActive_ThrowsEntityNotAvailableException()
        {
            //Arrange
            var rentalDTO = _fixture.Build<RentalDTO>()
                                    .With(r => r.StartDate, DateTime.Now.AddDays(3))
                                    .With(r => r.EndDate, DateTime.Now.AddDays(5))
                                    .Create();

            var client = _fixture.Build<Client>()
                                 .OmitAutoProperties()
                                 .With(c => c.ID, rentalDTO.Client.ID)
                                 .With(c => c.IsActive, false)
                                 .Create();

            _mockValidator.Setup(mv => mv.ValidateAsync(It.IsAny<RentalDTO>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult());

            _mockDBContext.Setup(mdbc => mdbc.Clients.FindAsync(It.IsAny<int>())).ReturnsAsync(client);

            //Act
            Func<Task> act = async () => await _sut.CreateRentalAsync(rentalDTO);

            //Assert
            await act.Should().ThrowAsync<EntityNotAvailableException>();
        }


        [Test]
        [Category("CreateRentalAsync")]
        public async Task CreateRentalAsync_WhenVehicleDoesNotExist_ThrowsEntityNotFoundException()
        {
            //Arrange
            var rentalDTO = _fixture.Build<RentalDTO>()
                                 .With(r => r.StartDate, DateTime.Now.AddDays(3))
                                 .With(r => r.EndDate, DateTime.Now.AddDays(5))
                                 .Create();

            var clientDTO = _fixture.Create<ClientDTO>();

            var client = GetClientFromDTO(clientDTO);

            _mockValidator.Setup(mv => mv.ValidateAsync(It.IsAny<RentalDTO>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult());

            _mockDBContext.Setup(mdbc => mdbc.Clients.FindAsync(It.IsAny<int>())).ReturnsAsync(client);
            _mockDBContext.Setup(mdbc => mdbc.Vehicles.FindAsync(It.IsAny<int>())).ReturnsAsync((Vehicle)null);

            //Act
            Func<Task> act = async () => await _sut.CreateRentalAsync(rentalDTO);

            //Assert
            await act.Should().ThrowAsync<EntityNotFoundException>();
        }

        [Test]
        [Category("CreateRentalAsync")]
        public async Task CreateRentalAsync_WhenVehicleIsNotActive_ThrowsEntityNotAvailableException()
        {
            //Arrange
            var vehicleDTO = _fixture.Create<VehicleDTO>();

            var vehicle = GetVehicleFromDTO(vehicleDTO);
            vehicle.IsActive = false;

            var clientDTO = _fixture.Create<ClientDTO>();

            var client = GetClientFromDTO(clientDTO);

            var rentalDTO = _fixture.Build<RentalDTO>()
                                    .With(r => r.Vehicle, vehicleDTO)
                                    .With(r => r.Client, clientDTO)
                                    .With(r => r.StartDate, DateTime.Now.AddDays(3))
                                    .With(r => r.EndDate, DateTime.Now.AddDays(13))
                                    .Create();

            _mockValidator.Setup(mv => mv.ValidateAsync(It.IsAny<RentalDTO>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult());

            _mockDBContext.Setup(mdbc => mdbc.Vehicles).ReturnsDbSet(new List<Vehicle> { vehicle });
            _mockDBContext.Setup(mdbc => mdbc.Clients.FindAsync(It.IsAny<int>())).ReturnsAsync(client);

            //Act
            Func<Task> act = async () => await _sut.CreateRentalAsync(rentalDTO);

            //Assert
            await act.Should().ThrowAsync<EntityNotAvailableException>();
        }

        [Test]
        [Category("CreateRentalAsync")]
        public async Task CreateRentalAsync_WhenVehicleIsRented_ThrowsVehicleInRentalException()
        {
            //Arrange
            var vehicleDTO = _fixture.Create<VehicleDTO>();

            var vehicle = GetVehicleFromDTO(vehicleDTO);
            vehicle.IsActive = true;
            vehicle.Rentals = new List<Rental>();

            var clientDTO = _fixture.Create<ClientDTO>();

            var client = GetClientFromDTO(clientDTO);

            var rentalDTO = _fixture.Build<RentalDTO>()
                                    .With(r => r.Vehicle, vehicleDTO)
                                    .With(r => r.Client, clientDTO)
                                    .With(r => r.StartDate, DateTime.Now.AddDays(3))
                                    .With(r => r.EndDate, DateTime.Now.AddDays(13))
                                    .Create();

            var rental = _fixture.Build<Rental>()
                                 .With(r => r.Vehicle, vehicle)
                                 .With(r => r.Client, client)
                                 .With(r => r.StartDate, rentalDTO.StartDate)
                                 .With(r => r.EndDate, rentalDTO.EndDate)
                                 .Create();

            vehicle.Rentals.Add(rental);

            _mockValidator.Setup(mv => mv.ValidateAsync(It.IsAny<RentalDTO>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult());

            _mockDBContext.Setup(mdbc => mdbc.Vehicles).ReturnsDbSet(new List<Vehicle> { vehicle });
            _mockDBContext.Setup(mdbc => mdbc.Clients.FindAsync(It.IsAny<int>())).ReturnsAsync(client);

            //Act
            Func<Task> act = async () => await _sut.CreateRentalAsync(rentalDTO);

            //Assert
            await act.Should().ThrowAsync<VehicleRentedException>();
        }

        [Test]
        [Category("CancelRentalAsync")]
        public async Task CancelRentalAsync_WhenRentalExistsAndIsActive_CancelsRental()
        {
            //Arrange
            int rentalIdToCancel = 1;
            var rental = _fixture.Build<Rental>()
                                 .OmitAutoProperties()
                                 .With(r => r.ID, rentalIdToCancel)
                                 .With(r => r.IsActive, true)
                                 .Create();

            _mockDBContext.Setup(mdbc => mdbc.Rentals.FindAsync(It.IsAny<int>())).ReturnsAsync(rental);

            //Act
            await _sut.CancelRentalAsync(rentalIdToCancel);

            //Assert
            _mockDBContext.Verify(mdbc => mdbc.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            rental.IsActive.Should().BeFalse();
        }

        [Test]
        [Category("CancelRentalAsync")]
        public async Task CancelRentalAsync_WhenRentalExistsAndIsNotActive_DoesNothing()
        {
            //Arrange
            int rentalIdToCancel = 1;
            var rental = _fixture.Build<Rental>()
                                 .OmitAutoProperties()
                                 .With(r => r.ID, rentalIdToCancel)
                                 .With(r => r.IsActive, false)
                                 .Create();

            _mockDBContext.Setup(mdbc => mdbc.Rentals.FindAsync(It.IsAny<int>())).ReturnsAsync(rental);

            //Act
            await _sut.CancelRentalAsync(rentalIdToCancel);

            //Assert
            _mockDBContext.Verify(mdbc => mdbc.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        [Category("CancelRentalAsync")]
        public async Task CancelRentalAsync_WhenRentalDoesNotExists_ThrowsEntityNotFoundException()
        {
            //Arrange
            int id = 1;

            //Act
            Func<Task> act = async () => await _sut.CancelRentalAsync(id);

            //Assert
            await act.Should().ThrowAsync<EntityNotFoundException>();
        }
    }
}
