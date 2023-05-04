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
    public class VehicleDTOValidatorTests
    {
        private VehicleDTOValidator _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new VehicleDTOValidator();
        }

        [Test]
        [Category("ValidateAsync")]
        public async Task ValidateAsync_WhenVehicleDTOIsValid_ReturnsIsValid()
        {
            //Arrange
            var vehicleDTO = new VehicleDTO()
            {
                Make = "Toyota",
                Model = "Corolla",
                ChassisNumber = "ABC123",
                PricePerDay = 10,
            };

            //Act
            var result = await _sut.ValidateAsync(vehicleDTO);

            //Assert
            result.IsValid.Should().BeTrue();
        }

        [TestCase(null, "model", "chassis")]
        [TestCase("make", null, "chassis")]
        [TestCase("make", "model", null)]
        [Category("ValidateAsync")]
        public async Task ValidateAsync_WhenAnyVehicleDTOPropertyIsNull_ReturnsIsNotValid(string make, string model, string chassisNumber)
        {
            //Arrange
            var vehicleDTO = new VehicleDTO()
            {
                Make = make,
                Model = model,
                ChassisNumber = chassisNumber,
                PricePerDay = 10,
            };

            //Act
            var result = await _sut.ValidateAsync(vehicleDTO);

            //Assert
            result.IsValid.Should().BeFalse();
        }

        [TestCase("", "model", "chassis")]
        [TestCase("make", "", "chassis")]
        [TestCase("make", "model", "")]
        [Category("ValidateAsync")]
        public async Task ValidateAsync_WhenAnyVehicleDTOPropertyIsEmptyString_ReturnsIsNotValid(string make, string model, string chassisNumber)
        {
            //Arrange
            var vehicleDTO = new VehicleDTO()
            {
                Make = make,
                Model = model,
                ChassisNumber = chassisNumber,
                PricePerDay = 10,
            };

            //Act
            var result = await _sut.ValidateAsync(vehicleDTO);

            //Assert
            result.IsValid.Should().BeFalse();
        }

        [Test]
        [Category("ValidateAsync")]
        public async Task ValidateAsync_WhenPriceIs0_ReturnsIsNotValid()
        {
            //Arrange
            var vehicleDTO = new VehicleDTO()
            {
                Make = "Toyota",
                Model = "Corolla",
                ChassisNumber = "ABC123",
                PricePerDay = 0,
            };

            //Act
            var result = await _sut.ValidateAsync(vehicleDTO);

            //Assert
            result.IsValid.Should().BeFalse();
        }

        [Test]
        [Category("ValidateAsync")]
        public async Task ValidateAsync_WhenPriceIsNegative_ReturnsIsNotValid()
        {
            //Arrange
            var vehicleDTO = new VehicleDTO()
            {
                Make = "Toyota",
                Model = "Corolla",
                ChassisNumber = "ABC123",
                PricePerDay = -10,
            };

            //Act
            var result = await _sut.ValidateAsync(vehicleDTO);

            //Assert
            result.IsValid.Should().BeFalse();
        }
    }
}
