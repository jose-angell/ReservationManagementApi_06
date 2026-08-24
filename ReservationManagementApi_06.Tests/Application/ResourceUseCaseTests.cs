using ReservationManagementApi_06.Application;
using ReservationManagementApi_06.Domain;
using ReservationManagementApi_06.Exceptions;
using ReservationManagementApi_06.Tests.TestSupport;

namespace ReservationManagementApi_06.Tests.Application
{
    public class ResourceUseCaseTests
    {
        [Fact]
        public async Task Availability_ShouldReturnAvailable_WhenResourceHasNoConflictingReservations()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var baseTime = DateTime.UtcNow.Date.AddDays(1).AddHours(10);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);

            var existingReservation = new Reservation(customer.Id, resource.Id, baseTime.AddHours(2), baseTime.AddHours(3), 120m);

            context.Customers.Add(customer);
            context.Resources.Add(resource);

            context.Reservations.Add(existingReservation);

            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new ResourceUseCase(context);

            var startTime = baseTime.AddHours(1);
            var endTime = baseTime.AddHours(2);

            // Act
            var result = await useCase.Availability(resource.Id, startTime, endTime);

            // Assert
            Assert.True(result.IsAvailable);

        }
        [Fact]
        public async Task Availability_ShouldReturnNotAvailable_WhenPendingReservationOverlaps()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var baseTime = DateTime.UtcNow.Date.AddDays(1).AddHours(10);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);

            var existingReservation = new Reservation(customer.Id, resource.Id, baseTime.AddHours(1), baseTime.AddHours(2), 120m);

            context.Customers.Add(customer);
            context.Resources.Add(resource);

            context.Reservations.Add(existingReservation);

            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new ResourceUseCase(context);

            var startTime = baseTime.AddHours(1);
            var endTime = baseTime.AddHours(2);

            // Act
            var result = await useCase.Availability(resource.Id, startTime, endTime);

            // Assert
            Assert.False(result.IsAvailable);

        }
        [Fact]
        public async Task Availability_ShouldReturnNotAvailable_WhenConfirmedReservationOverlaps()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var baseTime = DateTime.UtcNow.Date.AddDays(1).AddHours(10);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);

            var existingReservation = new Reservation(customer.Id, resource.Id, baseTime.AddHours(1), baseTime.AddHours(2), 120m);
            existingReservation.Confirm();
            context.Customers.Add(customer);
            context.Resources.Add(resource);

            context.Reservations.Add(existingReservation);

            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new ResourceUseCase(context);

            var startTime = baseTime.AddHours(1);
            var endTime = baseTime.AddHours(2);

            // Act
            var result = await useCase.Availability(resource.Id, startTime, endTime);

            // Assert
            Assert.False(result.IsAvailable);

        }
        [Fact]
        public async Task Availability_ShouldReturnAvailable_WhenOverlappingReservationIsCancelled()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var baseTime = DateTime.UtcNow.Date.AddDays(1).AddHours(10);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);

            var existingReservation = new Reservation(customer.Id, resource.Id, baseTime.AddHours(1), baseTime.AddHours(2), 120m);
            existingReservation.Cancel();

            context.Customers.Add(customer);
            context.Resources.Add(resource);

            context.Reservations.Add(existingReservation);

            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new ResourceUseCase(context);

            var startTime = baseTime.AddHours(1);
            var endTime = baseTime.AddHours(2);

            // Act
            var result = await useCase.Availability(resource.Id, startTime, endTime);

            // Assert
            Assert.True(result.IsAvailable);

        }
        [Fact]
        public async Task Availability_ShouldReturnAvailable_WhenOverlappingReservationIsCompleted()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var baseTime = DateTime.UtcNow.Date.AddDays(1).AddHours(10);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);

            var existingReservation = new Reservation(customer.Id, resource.Id, baseTime.AddHours(1), baseTime.AddHours(2), 120m);
            existingReservation.Confirm();
            existingReservation.Complete();

            context.Customers.Add(customer);
            context.Resources.Add(resource);

            context.Reservations.Add(existingReservation);

            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new ResourceUseCase(context);

            var startTime = baseTime.AddHours(1);
            var endTime = baseTime.AddHours(2);

            // Act
            var result = await useCase.Availability(resource.Id, startTime, endTime);

            // Assert
            Assert.True(result.IsAvailable);

        }
        [Fact]
        public async Task Availability_ShouldThrowNotFoundException_WhenResourceDoesNotExist()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var baseTime = DateTime.UtcNow.Date.AddDays(1).AddHours(10);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);

            var existingReservation = new Reservation(customer.Id, resource.Id, baseTime.AddHours(1), baseTime.AddHours(2), 120m);
            
            context.Customers.Add(customer);
            context.Resources.Add(resource);

            context.Reservations.Add(existingReservation);

            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new ResourceUseCase(context);

            var startTime = baseTime.AddHours(1);
            var endTime = baseTime.AddHours(2);

            // Act
            Func<Task> act = () => useCase.Availability(Guid.NewGuid(), startTime, endTime);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);

        }
        [Fact]
        public async Task Availability_ShouldThrowConflictException_WhenDatesAreInvalid()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var baseTime = DateTime.UtcNow.Date.AddDays(1).AddHours(10);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);

            var existingReservation = new Reservation(customer.Id, resource.Id, baseTime.AddHours(1), baseTime.AddHours(2), 120m);

            context.Customers.Add(customer);
            context.Resources.Add(resource);

            context.Reservations.Add(existingReservation);

            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new ResourceUseCase(context);

            var startTime = baseTime.AddHours(1);
            var endTime = baseTime.AddHours(1);

            // Act
            Func<Task> act = () => useCase.Availability(resource.Id, startTime, endTime);

            // Assert
            await Assert.ThrowsAsync<ConflictException>(act);

        }
        [Fact]
        public async Task Availability_ShouldReturnAvailable_WhenExistingReservationEndsExactlyAtRequestedStart()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var baseTime = DateTime.UtcNow.Date.AddDays(1).AddHours(10);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);

            var existingReservation = new Reservation(customer.Id, resource.Id, baseTime.AddHours(1), baseTime.AddHours(2), 120m);
            existingReservation.Confirm();
            existingReservation.Complete();

            context.Customers.Add(customer);
            context.Resources.Add(resource);

            context.Reservations.Add(existingReservation);

            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new ResourceUseCase(context);

            var startTime = baseTime.AddHours(2);
            var endTime = baseTime.AddHours(3);

            // Act
            var result = await useCase.Availability(resource.Id, startTime, endTime);

            // Assert
            Assert.True(result.IsAvailable);

        }
        [Fact]
        public async Task GetAvailableResources_ShouldReturnActiveResources_WhenTheyHaveNoReservations()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);

            context.Resources.Add(resource);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new ResourceUseCase(context);

            var startTime = DateTime.UtcNow.Date.AddDays(1).AddHours(10);
            var endTime = startTime.AddHours(2);

            // Act
            var result = await useCase.GetAvailableResources(startTime, endTime);

            // Assert
            var item = Assert.Single(result);
            Assert.Equal(resource.Id, item.Id);
            Assert.True(item.IsActive);
        }
        [Fact]
        public async Task GetAvailableResources_ShouldExcludeResource_WhenPendingReservationOverlaps()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var baseTime = DateTime.UtcNow.Date.AddDays(1).AddHours(10);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);

            var reservation = new Reservation(
                customer.Id,
                resource.Id,
                baseTime,
                baseTime.AddHours(2),
                240m);

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Reservations.Add(reservation);

            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new ResourceUseCase(context);

            var startTime = baseTime.AddHours(1);
            var endTime = baseTime.AddHours(3);

            // Act
            var result = await useCase.GetAvailableResources(startTime, endTime);

            // Assert
            Assert.Empty(result);
        }
        [Fact]
        public async Task GetAvailableResources_ShouldIncludeResource_WhenReservationBelongsToAnotherResource()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var baseTime = DateTime.UtcNow.Date.AddDays(1).AddHours(10);

            var customer = new Customer("José Gallardo", "jose@test.com");

            var occupiedResource = new Resource("Sala A", "Sala ocupada", 15, 120m);
            var availableResource = new Resource("Sala B", "Sala disponible", 10, 100m);

            var reservation = new Reservation(
                customer.Id,
                occupiedResource.Id,
                baseTime,
                baseTime.AddHours(2),
                240m);

            context.Customers.Add(customer);
            context.Resources.Add(occupiedResource);
            context.Resources.Add(availableResource);
            context.Reservations.Add(reservation);

            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new ResourceUseCase(context);

            // Act
            var result = await useCase.GetAvailableResources(baseTime, baseTime.AddHours(2));

            // Assert
            var items = result.ToList();

            Assert.Single(items);
            Assert.Equal(availableResource.Id, items[0].Id);
        }
        [Fact]
        public async Task GetAvailableResources_ShouldIncludeResource_WhenExistingReservationEndsExactlyAtRequestedStart()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var baseTime = DateTime.UtcNow.Date.AddDays(1).AddHours(10);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);

            var reservation = new Reservation(
                customer.Id,
                resource.Id,
                baseTime,
                baseTime.AddHours(2),
                240m);

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Reservations.Add(reservation);

            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new ResourceUseCase(context);

            var startTime = baseTime.AddHours(2);
            var endTime = baseTime.AddHours(4);

            // Act
            var result = await useCase.GetAvailableResources(startTime, endTime);

            // Assert
            var item = Assert.Single(result);
            Assert.Equal(resource.Id, item.Id);
        }
    }
}
