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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using NUnit.Framework.Constraints;
using System.Linq.Expressions;

namespace CarRentalTests.Services
{
    [TestFixture]
    public class VehicleServicesTests
    {
        private IVehicleService _sut;
        private Fixture _fixture;

        private Mock<RentalContext> _mockDBContext;
        private Mock<ILogger<VehicleService>> _mockLogger;
        private Mock<IValidator<VehicleDTO>> _mockValidator;

        [SetUp]
        public void Setup()
        {
            _mockDBContext = new Mock<RentalContext>();
            _mockDBContext.Setup(mdbc => mdbc.Vehicles).ReturnsDbSet(new List<Vehicle>());
            _mockDBContext.Setup(mdbc => mdbc.Rentals).ReturnsDbSet(new List<Rental>());

            _mockLogger = new Mock<ILogger<VehicleService>>();
            _mockValidator = new Mock<IValidator<VehicleDTO>>();

            _sut = new VehicleService(_mockLogger.Object, _mockDBContext.Object, _mockValidator.Object);
            _fixture = new Fixture();
        }

        private Vehicle GetVehicleFromDTO(VehicleDTO dto) 
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
        [Category("CreateVehicleAsync")]
        public async Task CreateVehicleAsync_WithValidVehicle_ReturnsCreatedVehicle()
        {
            // Arrange
            var vehicleDTO = _fixture.Create<VehicleDTO>();

            _mockValidator.Setup(mv => mv.ValidateAsync(It.IsAny<VehicleDTO>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult());

            // Act
            var result = await _sut.CreateVehicleAsync(vehicleDTO);

            // Assert
            result.ID.Should().NotBe(null);
            result.Model.Should().Be(vehicleDTO.Model);
            result.ChassisNumber.Should().Be(vehicleDTO.ChassisNumber);
            result.PricePerDay.Should().Be(vehicleDTO.PricePerDay);
        }

        [Test]
        [Category("CreateVehicleAsync")]
        public async Task CreateVehicleAsync_WhenVehicleIsInvalid_ThrowsValidationException()
        {
            // Arrange
            var vehicleDTO = _fixture.Build<VehicleDTO>()
                                     .Without(v => v.ChassisNumber)
                                     .Create();

            _mockValidator.Setup(mv => mv.ValidateAsync(It.IsAny<VehicleDTO>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult(new List<ValidationFailure>() { new ValidationFailure() }));

            // Act
            Func<Task> action = async () => await _sut.CreateVehicleAsync(vehicleDTO);

            // Assert
            await action.Should().ThrowAsync<ValidationException>();
        }

        [Test]
        [Category("CreateVehicleAsync")]
        public async Task CreateVehicleAsync_WhenVehicleAlreadyExists_ThrowsEntityExistsException()
        {
            // Arrange
            var vehicleDTO = _fixture.Create<VehicleDTO>();

            var vehicle = GetVehicleFromDTO(vehicleDTO);

            _mockValidator.Setup(mv => mv.ValidateAsync(It.IsAny<VehicleDTO>(), It.IsAny<CancellationToken>()))
                          .ReturnsAsync(new ValidationResult());

            _mockDBContext.Setup(mdbc => mdbc.Vehicles)
                          .ReturnsDbSet(new List<Vehicle>() { vehicle });

            // Act
            Func<Task> action = async () => await _sut.CreateVehicleAsync(vehicleDTO);

            // Assert
            await action.Should().ThrowAsync<EntityExistsException>();
        }

        [Test]
        [Category("DeleteVehicleAsync")]
        public async Task DeleteVehicleAsync_WhenVehicleExistsAndIsNotInRentalAndIsActive_VehicleIsMarkedAsNoLongerActive()
        {
            // Arrange
            var expected = _fixture.Create<VehicleDTO>();
            var vehicle = GetVehicleFromDTO(expected);
            vehicle.Rentals = new List<Rental>();

            _mockDBContext.Setup(mdbc => mdbc.Vehicles)
                          .ReturnsDbSet(new List<Vehicle>() { vehicle });

            // Act
            await _sut.DeleteVehicleAsync(vehicle.ID);

            // Assert
            _mockDBContext.Verify(mdbc => mdbc.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            vehicle.IsActive.Should().BeFalse();
        }

        [Test]
        [Category("DeleteVehicleAsync")]
        public async Task DeleteVehicleAsync_WhenVehicleExistsAndIsNotInRentalAndIsNotActive_DoesNothing()
        {
            // Arrange
            var expected = _fixture.Create<VehicleDTO>();
            var vehicle = GetVehicleFromDTO(expected);
            vehicle.Rentals = new List<Rental>();
            vehicle.IsActive = false;

            _mockDBContext.Setup(mdbc => mdbc.Vehicles)
                          .ReturnsDbSet(new List<Vehicle>() { vehicle });

            // Act
            await _sut.DeleteVehicleAsync(vehicle.ID);

            // Assert
            _mockDBContext.Verify(mdbc => mdbc.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        [Category("DeleteVehicleAsync")]
        public async Task DeleteVehicleAsync_WhenVehicleDoesNotExists_ThrowsEntityNotFoundException()
        {
            // Arrange
            var vehicleId = 1;

            // Act
            Func<Task> action = async () => await _sut.DeleteVehicleAsync(vehicleId);

            // Assert
            await action.Should().ThrowAsync<EntityNotFoundException>();
        }

        [Test]
        [Category("DeleteVehicleAsync")]
        public async Task DeleteVehicleAsync_WhenVehicleIsRented_ThrowsVehicleInRentalException()
        {
            // Arrange
            var vehicle = _fixture.Build<Vehicle>()
                                  .Without(v => v.Rentals)
                                  .Create();

            var rental = _fixture.Build<Rental>()
                                 .With(r => r.VehicleID, vehicle.ID)
                                 .Without(r => r.Vehicle)
                                 .Without(r => r.Client)
                                 .With(r => r.StartDate, DateTime.Now.AddDays(2))
                                 .With(r => r.EndDate, DateTime.Now.AddDays(5))
                                 .With(r => r.IsActive, true)
                                 .Create();

            vehicle.Rentals = new List<Rental> { rental };

            _mockDBContext.Setup(mdbc => mdbc.Vehicles)
                          .ReturnsDbSet(new List<Vehicle>() { vehicle });

            // Act
            Func<Task> action = async () => await _sut.DeleteVehicleAsync(vehicle.ID);

            // Assert
            await action.Should().ThrowAsync<VehicleRentedException>();
        }
    }
}
