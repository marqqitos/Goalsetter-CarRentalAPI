using AutoFixture;
using CarRental.DTOs;
using CarRental.Validators;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRentalTests.Validators
{
    [TestFixture]
    public class RentalDTOValidatorTests
    {
        private RentalDTOValidator _sut;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _sut = new RentalDTOValidator();
            _fixture = new Fixture();
        }

        [Test]
        [Category("ValidateAsync")]
        public async Task ValidateAsync_WhenRentalDTOIsValid_ReturnsIsValid()
        {
            //Arrange
            var clientDTO = _fixture.Build<ClientDTO>()
                                    .With(c => c.Email, "email@example.com")
                                    .Create();

            var vehicleDTO = _fixture.Build<VehicleDTO>()
                                     .With(v => v.PricePerDay, 10)
                                     .Create();

            var rentalDTO = new RentalDTO()
            {
                Client = clientDTO,
                Vehicle = vehicleDTO,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(11)
            };

            //Act
            var result = await _sut.ValidateAsync(rentalDTO);

            //Assert
            result.IsValid.Should().BeTrue();
        }

        [Test]
        [Category("ValidateAsync")]
        public async Task ValidateAsync_WhenStartDateIsInThePast_ReturnsIsNotValid()
        {
            //Arrange
            var clientDTO = _fixture.Create<ClientDTO>();
            var vehicleDTO = _fixture.Create<VehicleDTO>();

            var rentalDTO = new RentalDTO()
            {
                Client = clientDTO,
                Vehicle = vehicleDTO,
                StartDate = DateTime.Today.AddDays(-1),
                EndDate = DateTime.Today.AddDays(11)
            };

            //Act
            var result = await _sut.ValidateAsync(rentalDTO);

            //Assert
            result.IsValid.Should().BeFalse();
        }

        [Test]
        [Category("ValidateAsync")]
        public async Task ValidateAsync_WhenStartDateIsAfterEndDate_ReturnsIsNotValid()
        {
            //Arrange
            var clientDTO = _fixture.Create<ClientDTO>();
            var vehicleDTO = _fixture.Create<VehicleDTO>();

            var rentalDTO = new RentalDTO()
            {
                Client = clientDTO,
                Vehicle = vehicleDTO,
                StartDate = DateTime.Today.AddDays(21),
                EndDate = DateTime.Today.AddDays(11)
            };

            //Act
            var result = await _sut.ValidateAsync(rentalDTO);

            //Assert
            result.IsValid.Should().BeFalse();
        }

        [Test]
        [Category("ValidateAsync")]
        public async Task ValidateAsync_WhenRentalDateIsInThePast_ReturnsIsNotValid()
        {
            //Arrange
            var clientDTO = _fixture.Create<ClientDTO>();
            var vehicleDTO = _fixture.Create<VehicleDTO>();

            var rentalDTO = new RentalDTO()
            {
                Client = clientDTO,
                Vehicle = vehicleDTO,
                StartDate = DateTime.Today.AddDays(-21),
                EndDate = DateTime.Today.AddDays(-11)
            };

            //Act
            var result = await _sut.ValidateAsync(rentalDTO);

            //Assert
            result.IsValid.Should().BeFalse();
        }

        [Test]
        [Category("ValidateAsync")]
        public async Task ValidateAsync_WhenStartDateAndEndDateAreTheSame_ReturnsIsNotValid()
        {
            //Arrange
            var clientDTO = _fixture.Create<ClientDTO>();
            var vehicleDTO = _fixture.Create<VehicleDTO>();

            var rentalDTO = new RentalDTO()
            {
                Client = clientDTO,
                Vehicle = vehicleDTO,
                StartDate = DateTime.Today.AddDays(1),
                EndDate = DateTime.Today.AddDays(1)
            };

            //Act
            var result = await _sut.ValidateAsync(rentalDTO);

            //Assert
            result.IsValid.Should().BeFalse();
        }
    }
}
