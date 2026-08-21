using ReservationManagementApi_06.Application;
using ReservationManagementApi_06.Domain;
using ReservationManagementApi_06.Dtos.Reservation;
using ReservationManagementApi_06.Tests.TestSupport;


namespace ReservationManagementApi_06.Tests.Application
{
    public class ReservationUseCaseTests
    {
        [Fact]
        public async Task Create_ShouldCreateReservation_WhenRequestIsValid()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 10, 100m);

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            await context.SaveChangesAsync();

            var useCase = new ReservationUseCase(context);

            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var request = new CreateReservation
            {
                CustomerId = customer.Id,
                ResourceId = resource.Id,
                StartDateTime = startTime,
                EndDateTime = endTime
            };

            // Act
            var result = await useCase.Create(request);

            // Assert
            Assert.Equal(customer.Id, result.CustomerId);
            Assert.Equal(resource.Id, result.ResourceId);
            Assert.Equal(StatusReservation.Pending, result.Status);
            Assert.Equal(200m, result.TotalPrice);

            var reservationInDb = await context.Reservations.FindAsync(result.Id);
            Assert.NotNull(reservationInDb);
        }
    }
}
