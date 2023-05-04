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
    public class VehicleTests
    {
        private Vehicle _sut;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _sut = new Vehicle()
            {
                ID = 1,
                Rentals = new List<Rental>()
            };

            _fixture = new Fixture();
        }

        [Test]
        [Category("IsRented")]
        public void IsRented_WhenRentalIsActiveAndRentalIsStillGoing_ReturnsTrue()
        {
            // Arrange
            var rental = _fixture.Build<Rental>()
                                 .OmitAutoProperties()
                                 .With(r => r.VehicleID, _sut.ID)
                                 .With(r => r.EndDate, DateTime.Today.AddDays(2))
                                 .With(r => r.IsActive, true)
                                 .Create();

            _sut.Rentals.Add(rental);

            // Act
            var result = _sut.IsRented();

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        [Category("IsRented")]
        public void IsRented_WhenRentalIsActiveAndRentalIsInTheFuture_ReturnsTrue()
        {
            // Arrange
            var rental = _fixture.Build<Rental>()
                                 .OmitAutoProperties()
                                 .With(r => r.VehicleID, _sut.ID)
                                 .With(r => r.StartDate, DateTime.Today.AddDays(2))
                                 .With(r => r.EndDate, DateTime.Today.AddDays(5))
                                 .With(r => r.IsActive, true)
                                 .Create();

            _sut.Rentals.Add(rental);

            // Act
            var result = _sut.IsRented();

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        [Category("IsRented")]
        public void IsRented_WhenRentalIsActiveAndRentalEndsToday_ReturnsTrue()
        {
            // Arrange
            var rental = _fixture.Build<Rental>()
                                 .OmitAutoProperties()
                                 .With(r => r.VehicleID, _sut.ID)
                                 .With(r => r.EndDate, DateTime.Today)
                                 .With(r => r.IsActive, true)
                                 .Create();

            _sut.Rentals.Add(rental);

            // Act
            var result = _sut.IsRented();

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        [Category("IsRented")]
        public void IsRented_WhenRentalIsActiveAndRentalEnded_ReturnsFalse()
        {
            // Arrange
            var rental = _fixture.Build<Rental>()
                                 .OmitAutoProperties()
                                 .With(r => r.VehicleID, _sut.ID)
                                 .With(r => r.EndDate, DateTime.Today.AddDays(-2))
                                 .With(r => r.IsActive, true)
                                 .Create();

            _sut.Rentals.Add(rental);

            // Act
            var result = _sut.IsRented();

            // Assert
            result.Should().BeFalse();
        }


        [Test]
        [Category("IsRented")]
        public void IsRented_WhenVehicleIsNotRented_ReturnsFalse()
        {
            // Arrange

            // Act
            var result = _sut.IsRented();

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        [Category("IsRented")]
        public void IsRented_WhenVehicleIsRentedButRentalIsNotActive_ReturnsFalse()
        {
            // Arrange
            var rental = _fixture.Build<Rental>()
                                 .OmitAutoProperties()
                                 .With(r => r.VehicleID, _sut.ID)
                                 .With(r => r.EndDate, DateTime.Today.AddDays(2))
                                 .With(r => r.IsActive, false)
                                 .Create();

            _sut.Rentals.Add(rental);

            // Act
            var result = _sut.IsRented();

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        [Category("IsRented")]
        public void IsRented_WhenVehicleHasInactiveFutureRental_ReturnsFalse()
        {
            // Arrange
            var rental = _fixture.Build<Rental>()
                                 .OmitAutoProperties()
                                 .With(r => r.VehicleID, _sut.ID)
                                 .With(r => r.StartDate, DateTime.Today.AddDays(2))
                                 .With(r => r.EndDate, DateTime.Today.AddDays(5))
                                 .With(r => r.IsActive, false)
                                 .Create();

            _sut.Rentals.Add(rental);

            // Act
            var result = _sut.IsRented();

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        [Category("IsAvailableForRental")]
        public void IsAvailableForRental_WhenVehicleIsAvailable_ReturnsTrue()
        {
            //Arrange
            var rental1 = _fixture.Build<Rental>()
                                  .OmitAutoProperties()
                                  .With(r => r.VehicleID, _sut.ID)
                                  .With(r => r.StartDate, DateTime.Now.AddDays(5))
                                  .With(r => r.EndDate, DateTime.Now.AddDays(10))
                                  .With(r => r.IsActive, true)
                                  .Create();

            var rental2 = _fixture.Build<Rental>()
                                  .OmitAutoProperties()
                                  .With(r => r.VehicleID, _sut.ID)
                                  .With(r => r.StartDate, DateTime.Now.AddDays(15))
                                  .With(r => r.EndDate, DateTime.Now.AddDays(20))
                                  .With(r => r.IsActive, true)
                                  .Create();

            var rental3 = _fixture.Build<Rental>()
                                  .OmitAutoProperties()
                                  .With(r => r.VehicleID, _sut.ID)
                                  .With(r => r.StartDate, DateTime.Now.AddDays(25))
                                  .With(r => r.EndDate, DateTime.Now.AddDays(30))
                                  .With(r => r.IsActive, true)
                                  .Create();

            _sut.Rentals.Add(rental1);
            _sut.Rentals.Add(rental2);
            _sut.Rentals.Add(rental3);

            var startDate = DateTime.Now.AddDays(1);
            var endDate = DateTime.Now.AddDays(2);

            //Act
            var result = _sut.IsAvailableForRental(startDate, endDate);

            //Assert
            result.Should().BeTrue();
        }

        [TestCase(0, 10)]
        [TestCase(0, 5)]
        [TestCase(5, 10)]
        [TestCase(4, 6)]
        [Category("IsAvailableForRental")]
        public void IsAvailableForRental_WhenRentIsOverlappedAndIsNotActive_ReturnsTrue(int startDateDaysToAdd, int endDateDaysToAdd)
        {
            //Arrange
            var rental1 = _fixture.Build<Rental>()
                                  .OmitAutoProperties()
                                  .With(r => r.VehicleID, _sut.ID)
                                  .With(r => r.StartDate, DateTime.Now.AddDays(3))
                                  .With(r => r.EndDate, DateTime.Now.AddDays(7))
                                  .With(r => r.IsActive, false)
                                  .Create();

            _sut.Rentals.Add(rental1);
                
            var startDate = DateTime.Now.AddDays(startDateDaysToAdd);
            var endDate = DateTime.Now.AddDays(endDateDaysToAdd);

            //Act
            var result = _sut.IsAvailableForRental(startDate, endDate);

            //Assert
            result.Should().BeTrue();
        }

        [TestCase(0, 10)]
        [TestCase(0, 5)]
        [TestCase(5, 10)]
        [TestCase(4, 6)]
        [Category("IsAvailableForRental")]
        public void IsAvailableForRental_WhenRentIsOverlappedAndIsActive_ReturnsFalse(int startDateDaysToAdd, int endDateDaysToAdd)
        {
            //Arrange
            var rental1 = _fixture.Build<Rental>()
                                  .OmitAutoProperties()
                                  .With(r => r.VehicleID, _sut.ID)
                                  .With(r => r.StartDate, DateTime.Now.AddDays(3))
                                  .With(r => r.EndDate, DateTime.Now.AddDays(7))
                                  .With(r => r.IsActive, true)
                                  .Create();

            _sut.Rentals.Add(rental1);

            var startDate = DateTime.Now.AddDays(startDateDaysToAdd);
            var endDate = DateTime.Now.AddDays(endDateDaysToAdd);

            //Act
            var result = _sut.IsAvailableForRental(startDate, endDate);

            //Assert
            result.Should().BeFalse();
        }
    }
}
