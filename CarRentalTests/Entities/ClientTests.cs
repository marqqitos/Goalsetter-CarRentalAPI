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
    public class ClientTests
    {
        private Client _sut;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _sut = new Client()
            {
                ID = 1,
                Rentals = new List<Rental>()
            };

            _fixture = new Fixture();
        }

        [Test]
        [Category("HasActiveRentals")]
        public void HasActiveRentals_WhenRentalIsActiveAndRentalIsStillGoing_ReturnsTrue()
        {
            // Arrange
            var rental = _fixture.Build<Rental>()
                                 .OmitAutoProperties()
                                 .With(r => r.ClientID, _sut.ID)
                                 .With(r => r.EndDate, DateTime.Today.AddDays(2))
                                 .With(r => r.IsActive, true)
                                 .Create();

            _sut.Rentals.Add(rental);

            // Act
            var result = _sut.HasActiveRentals();

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        [Category("HasActiveRentals")]
        public void HasActiveRentals_WhenRentalIsActiveAndRentalIsInTheFuture_ReturnsTrue()
        {
            // Arrange
            var rental = _fixture.Build<Rental>()
                                 .OmitAutoProperties()
                                 .With(r => r.ClientID, _sut.ID)
                                 .With(r => r.StartDate, DateTime.Today.AddDays(2))
                                 .With(r => r.EndDate, DateTime.Today.AddDays(5))
                                 .With(r => r.IsActive, true)
                                 .Create();

            _sut.Rentals.Add(rental);

            // Act
            var result = _sut.HasActiveRentals();

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        [Category("HasActiveRentals")]
        public void HasActiveRentals_WhenRentalIsActiveAndRentalEndsToday_ReturnsTrue()
        {
            // Arrange
            var rental = _fixture.Build<Rental>()
                                 .OmitAutoProperties()
                                 .With(r => r.ClientID, _sut.ID)
                                 .With(r => r.EndDate, DateTime.Today)
                                 .With(r => r.IsActive, true)
                                 .Create();

            _sut.Rentals.Add(rental);

            // Act
            var result = _sut.HasActiveRentals();

            // Assert
            result.Should().BeTrue();
        }

        [Test]
        [Category("HasActiveRentals")]
        public void HasActiveRentals_WhenRentalIsActiveAndRentalEnded_ReturnsFalse()
        {
            // Arrange
            var rental = _fixture.Build<Rental>()
                                 .OmitAutoProperties()
                                 .With(r => r.ClientID, _sut.ID)
                                 .With(r => r.EndDate, DateTime.Today.AddDays(-2))
                                 .With(r => r.IsActive, true)
                                 .Create();

            _sut.Rentals.Add(rental);

            // Act
            var result = _sut.HasActiveRentals();

            // Assert
            result.Should().BeFalse();
        }


        [Test]
        [Category("HasActiveRentals")]
        public void HasActiveRentals_WhenClientHasNoRentals_ReturnsFalse()
        {
            // Arrange

            // Act
            var result = _sut.HasActiveRentals();

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        [Category("HasActiveRentals")]
        public void HasActiveRentals_WhenClientHasInactiveOngoingRental_ReturnsFalse()
        {
            // Arrange
            var rental = _fixture.Build<Rental>()
                                 .OmitAutoProperties()
                                 .With(r => r.ClientID, _sut.ID)
                                 .With(r => r.EndDate, DateTime.Today.AddDays(2))
                                 .With(r => r.IsActive, false)
                                 .Create();

            _sut.Rentals.Add(rental);

            // Act
            var result = _sut.HasActiveRentals();

            // Assert
            result.Should().BeFalse();
        }

        [Test]
        [Category("HasActiveRentals")]
        public void HasActiveRentals_WhenClientHasInactiveFutureRental_ReturnsFalse()
        {
            // Arrange
            var rental = _fixture.Build<Rental>()
                                 .OmitAutoProperties()
                                 .With(r => r.ClientID, _sut.ID)
                                 .With(r => r.StartDate, DateTime.Today.AddDays(2))
                                 .With(r => r.EndDate, DateTime.Today.AddDays(4))
                                 .With(r => r.IsActive, false)
                                 .Create();

            _sut.Rentals.Add(rental);

            // Act
            var result = _sut.HasActiveRentals();

            // Assert
            result.Should().BeFalse();
        }
    }
}
