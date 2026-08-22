using ReservationManagementApi_06.Application;
using ReservationManagementApi_06.Domain;
using ReservationManagementApi_06.Dtos.Reservation;
using ReservationManagementApi_06.Exceptions;
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
        [Fact]
        public async Task Create_ShouldThrowNotFoundException_WhenCustomerDoesNotExist()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var resource = new Resource("Sala A", "Sala de juntas", 10, 100m);

            context.Resources.Add(resource);
            await context.SaveChangesAsync();

            var useCase = new ReservationUseCase(context);

            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var request = new CreateReservation
            {
                CustomerId = Guid.NewGuid(),
                ResourceId = resource.Id,
                StartDateTime = startTime,
                EndDateTime = endTime
            };

            // Act
            Func<Task> act = () => useCase.Create(request);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }
        [Fact]
        public async Task Create_ShouldThrowNotFoundException_WhenResourceDoesNotExist()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var customer = new Customer("José Gallardo", "jose@test.com");

            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var useCase = new ReservationUseCase(context);

            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var request = new CreateReservation
            {
                CustomerId = customer.Id,
                ResourceId = Guid.NewGuid(),
                StartDateTime = startTime,
                EndDateTime = endTime
            };
            // Act
            Func<Task> act = () => useCase.Create(request);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }
        [Fact]
        public async Task Create_ShouldThrowConflictException_WhenResourceIsInactive()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 10, 100m);
            resource.Deactivate();
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
            Func<Task> act = () => useCase.Create(request);

            // Assert
            await Assert.ThrowsAsync<ConflictException>(act);
        }
        [Fact]
        public async Task Create_ShouldThrowConflictException_WhenReservationOverlapsPendingReservation()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 10, 100m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Reservations.Add(existingReservation);

            await context.SaveChangesAsync();

            var useCase = new ReservationUseCase(context);


            var request = new CreateReservation
            {
                CustomerId = customer.Id,
                ResourceId = resource.Id,
                StartDateTime = startTime,
                EndDateTime = endTime
            };

            // Act
            Func<Task> act = () => useCase.Create(request);

            // Assert
            await Assert.ThrowsAsync<ConflictException>(act);
        }
        [Fact]
        public async Task Create_ShouldThrowConflictException_WhenReservationOverlapsConfirmedReservation()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 10, 100m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);
            existingReservation.Confirm();

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Reservations.Add(existingReservation);
            await context.SaveChangesAsync();

            var useCase = new ReservationUseCase(context);


            var request = new CreateReservation
            {
                CustomerId = customer.Id,
                ResourceId = resource.Id,
                StartDateTime = startTime,
                EndDateTime = endTime
            };

            // Act
            Func<Task> act = () => useCase.Create(request);

            // Assert
            await Assert.ThrowsAsync<ConflictException>(act);
        }
        [Fact]
        public async Task Create_ShouldCreateReservation_WhenOverlappingReservationIsCancelled()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 10, 100m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);
            existingReservation.Cancel();

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Reservations.Add(existingReservation);
            await context.SaveChangesAsync();

            var useCase = new ReservationUseCase(context);


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
        [Fact]
        public async Task Create_ShouldCreateReservation_WhenOverlappingReservationIsCompleted()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 10, 100m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);
            existingReservation.Confirm();
            existingReservation.Complete();

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Reservations.Add(existingReservation);
            await context.SaveChangesAsync();

            var useCase = new ReservationUseCase(context);


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
