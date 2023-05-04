using AutoFixture;
using CarRental.Entities;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarRentalTests.Entities
{
    [TestFixture]
    public class RentalTests
    {
        private Rental _sut;

        [SetUp]
        public void Setup()
        {
            _sut = new Rental();
        }

        [Test]
        [Category("CalculateRentalChargePrice")]
        public void CalculateRentalChargePrice_WhenPriceIs10AndRentLenghtIs10_Returns100()
        {
            //Arrange
            var vehicle = new Vehicle()
            {
                PricePerDay = 10
            };

            _sut.Vehicle = vehicle;
            _sut.StartDate = DateTime.Now.AddDays(1);
            _sut.EndDate = DateTime.Now.AddDays(11);

            //Act
            _sut.CalculateRentalChargePrice();

            //Assert
            _sut.RentalChargePrice.Should().BeApproximately(100M, 0.001M);
        }
    }
}
