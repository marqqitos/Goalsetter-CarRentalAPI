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
    public class ClientDTOValidatorTests
    {
        private ClientDTOValidator _sut;

        [SetUp]
        public void SetUp()
        {
            _sut = new ClientDTOValidator();
        }

        [Test]
        [Category("ValidateAsync")]
        public async Task ValidateAsync_WhenClientDTOIsValid_ReturnsIsValid()
        {
            //Arrange
            var clientDTO = new ClientDTO()
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "example@mail.com"
            };

            //Act
            var result = await _sut.ValidateAsync(clientDTO);

            //Assert
            result.IsValid.Should().BeTrue();
        }

        [TestCase(null, "Doe", "example@mail.com")]
        [TestCase("John", null, "example@mail.com")]
        [TestCase("John", "Doe", null)]
        [Category("ValidateAsync")]
        public async Task ValidateAsync_WhenAnyClientDTOPropertyIsNull_ReturnsIsNotValid(string firstName, string lastName, string email)
        {
            //Arrange
            var clientDTO = new ClientDTO()
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email
            };

            //Act
            var result = await _sut.ValidateAsync(clientDTO);

            //Assert
            result.IsValid.Should().BeFalse();
        }

        [TestCase("", "Doe", "example@mail.com")]
        [TestCase("John", "", "example@mail.com")]
        [TestCase("John", "Doe", "")]
        [Category("ValidateAsync")]
        public async Task ValidateAsync_WhenAnyClientDTOPropertyIsEmptyString_ReturnsIsNotValid(string firstName, string lastName, string email)
        {
            //Arrange
            var clientDTO = new ClientDTO()
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email
            };

            //Act
            var result = await _sut.ValidateAsync(clientDTO);

            //Assert
            result.IsValid.Should().BeFalse();
        }

        [TestCase("john")]
        [TestCase("john@mail@mail.com")]
        [Category("ValidateAsync")]
        public async Task ValidateAsync_WhenEmailHasBadFormat_ReturnsIsNotValid(string email)
        {
            //Arrange
            var clientDTO = new ClientDTO()
            {
                FirstName = "John",
                LastName = "Doe",
                Email = email
            };

            //Act
            var result = await _sut.ValidateAsync(clientDTO);

            //Assert
            result.IsValid.Should().BeFalse();
        }
    }
}
