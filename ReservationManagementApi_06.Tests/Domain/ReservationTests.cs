using ReservationManagementApi_06.Domain;
using ReservationManagementApi_06.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace ReservationManagementApi_06.Tests.Domain
{
    public class ReservationTests
    {
        [Fact]
        public void Create_ShouldReservation_Start_Pending()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var startTime = DateTimeOffset.Now;
            var endTime = DateTimeOffset.Now;
            decimal totalPrice = 100.22m;
            startTime = startTime.AddHours(1);
            endTime = endTime.AddHours(2);

            // Act
            var result = new Reservation(customerId, resourceId, startTime, endTime, totalPrice);

            // Assert
            Assert.Equal(StatusReservation.Pending, result.Status);
        }

        [Fact]
        public void CreateFail_StarteTime_After_EndTime()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var startTime = DateTimeOffset.Now;
            var endTime = DateTimeOffset.Now;
            decimal totalPrice = 100.22m;
            startTime = startTime.AddHours(2);
            endTime = endTime.AddHours(1);

            // Act and Assert

            Assert.Throws<DomainException>(() => new Reservation(customerId, resourceId, startTime, endTime, totalPrice));
        }
        [Fact]
        public void Confirm_Change_Pending_To_Confirmed()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var startTime = DateTimeOffset.Now;
            var endTime = DateTimeOffset.Now;
            decimal totalPrice = 100.22m;
            startTime = startTime.AddHours(1);
            endTime = endTime.AddHours(2);
            var reservation = new Reservation(customerId, resourceId, startTime, endTime, totalPrice);

            // Act
            reservation.Confirm();

            // Assert
            Assert.Equal(StatusReservation.Confirmed, reservation.Status);
        }
        [Fact]
        public void Confirm_Fail_If_StatusIsCancelled()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var startTime = DateTimeOffset.Now;
            var endTime = DateTimeOffset.Now;
            decimal totalPrice = 100.22m;
            startTime = startTime.AddHours(1);
            endTime = endTime.AddHours(2);
            var reservation = new Reservation(customerId, resourceId, startTime, endTime, totalPrice);
            reservation.Cancel();

            // Act
            Assert.Throws<DomainException>(() => reservation.Confirm());
        }
        [Fact]
        public void Complete_Fail_If_StatusIsPending()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var startTime = DateTimeOffset.Now;
            var endTime = DateTimeOffset.Now;
            decimal totalPrice = 100.22m;
            startTime = startTime.AddHours(1);
            endTime = endTime.AddHours(2);
            var reservation = new Reservation(customerId, resourceId, startTime, endTime, totalPrice);

            // Act
            Assert.Throws<DomainException>(() => reservation.Complete());
        }
    }
}
