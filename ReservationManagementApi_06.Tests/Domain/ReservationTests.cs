using ReservationManagementApi_06.Domain;
using ReservationManagementApi_06.Exceptions;

namespace ReservationManagementApi_06.Tests.Domain
{
    public class ReservationTests
    {
        private static Reservation CreatePendingReservation()
        {
            var startTime = DateTimeOffset.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(1);

            return new Reservation(
                Guid.NewGuid(),
                Guid.NewGuid(),
                startTime,
                endTime,
                100.22m);
        }
        [Fact]
        public void Constructor_ShouldSetStatusToPending_WhenReservationIsCreated()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var startTime = DateTimeOffset.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(1);
            decimal totalPrice = 100.22m;

            // Act
            var result = new Reservation(customerId, resourceId, startTime, endTime, totalPrice);

            // Assert
            Assert.Equal(StatusReservation.Pending, result.Status);
        }

        [Fact]
        public void Constructor_ShouldThrowDomainException_WhenStartDateTimeIsAfterEndDateTime()
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var startTime = DateTimeOffset.UtcNow.AddHours(2);
            var endTime = startTime.AddHours(-1);
            decimal totalPrice = 100.22m;

            // Act and Assert

            Assert.Throws<DomainException>(() => new Reservation(customerId, resourceId, startTime, endTime, totalPrice));
        }
        [Fact]
        public void Confirm_ShouldChangeStatusToConfirmed_WhenReservationIsPending()
        {
            // Arrange
            var reservation = CreatePendingReservation();

            // Act
            reservation.Confirm();

            // Assert
            Assert.Equal(StatusReservation.Confirmed, reservation.Status);
        }
        [Fact]
        public void Confirm_ShouldThrowDomainException_WhenReservationIsCancelled()
        {
            // Arrange
            var reservation = CreatePendingReservation();
            reservation.Cancel();

            // Act
            Assert.Throws<DomainException>(() => reservation.Confirm());
        }
        [Fact]
        public void Complete_ShouldThrowDomainException_WhenReservationIsPending()
        {
            // Arrange
            var reservation = CreatePendingReservation();

            // Act
            Assert.Throws<DomainException>(() => reservation.Complete());
        }
        [Fact]
        public void Constructor_ShouldThrowDomainException_WhenCustomerIdIsEmpty()
        {
            //Arrange
            var resourceId = Guid.NewGuid();
            var startTime = DateTimeOffset.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(1);
            decimal totalPrice = 100.22m;

            // Act and Assert
            Assert.Throws<DomainException>(() => new Reservation(Guid.Empty, resourceId, startTime, endTime, totalPrice));
        }
        [Fact]
        public void Constructor_ShouldThrowDomainException_WhenResourceIdIsEmpty()
        {
            //Arrange
            var customerId = Guid.NewGuid();
            var startTime = DateTimeOffset.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(1);
            decimal totalPrice = 100.22m;

            // Act and Assert
            Assert.Throws<DomainException>(() => new Reservation(customerId, Guid.Empty, startTime, endTime, totalPrice));
        }
        [Theory]
        [InlineData(0)]
        [InlineData(-100.22)]
        public void Constructor_ShouldThrowDomainException_WhenTotalPriceIsZeroOrNegative(double totalPriceInput)
        {
            // Arrange
            var customerId = Guid.NewGuid();
            var resourceId = Guid.NewGuid();
            var startTime = DateTimeOffset.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(1);

            // Convertimos el double a decimal aquí adentro
            decimal totalPrice = (decimal)totalPriceInput;

            // Act and Assert
            Assert.Throws<DomainException>(() => new Reservation(customerId, resourceId, startTime, endTime, totalPrice));
        }
        [Fact]
        public void Cancel_ShouldChangeStatusToCancelled_WhenReservationIsPending()
        {
            /// Arrange
            var reservation = CreatePendingReservation();

            // Act
            reservation.Cancel();

            // Assert
            Assert.Equal(StatusReservation.Cancelled, reservation.Status);
        }
        [Fact]
        public void Complete_ShouldChangeStatusToCompleted_WhenReservationIsConfirmed()
        {
            // Arrange
            var reservation = CreatePendingReservation();
            reservation.Confirm();
            // Act
            reservation.Complete();

            // Assert
            Assert.Equal(StatusReservation.Completed, reservation.Status);
        }
    }
}
