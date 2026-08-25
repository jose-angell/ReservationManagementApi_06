using FluentAssertions.Common;
using ReservationManagementApi_06.Application;
using ReservationManagementApi_06.Domain;
using ReservationManagementApi_06.Dtos.Resource;
using ReservationManagementApi_06.Exceptions;
using ReservationManagementApi_06.Tests.TestSupport;
using System.Reflection.PortableExecutable;

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
        [Fact]
        public async Task Create_ShouldCreateResource_WhenRequestIsValid()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var useCase = new ResourceUseCase(context);

            var request = new CreateResource
            {
                Name = "test",
                Description = "test",
                Capacity = 1,
                HourlyRate = 1.99m,
            };

            // Act
            var result = await useCase.Create(request);

            // Assert
            Assert.Equal(request.Name, result.Name);
            Assert.Equal(request.Description, result.Description);
            Assert.Equal(request.Capacity, result.Capacity);
            Assert.Equal(request.HourlyRate, result.HourlyRate);

            var reservationInDb = await context.Resources.FindAsync(result.Id);
            Assert.NotNull(reservationInDb);
        }
        [Fact]
        public async Task Create_ShouldThrowConflictException_WhenResourceNameAlreadyExists()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var resource = new Resource("Sala A", "Sala de juntas", 10, 100m);

            context.Resources.Add(resource);
            await context.SaveChangesAsync();

            var useCase = new ResourceUseCase(context);

            var request = new CreateResource
            {
                Name = "Sala A",
                Description = "test",
                Capacity = 1,
                HourlyRate = 1.99m,
            };

            // Act
            Func<Task> act = () => useCase.Create(request);

            await Assert.ThrowsAsync<ConflictException>(act);
        }
        [Fact]
        public async Task Create_ShouldThrowDomainException_WhenNameIsEmpty()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var useCase = new ResourceUseCase(context);

            var request = new CreateResource
            {
                Name = "",
                Description = "test",
                Capacity = 1,
                HourlyRate = 1.99m,
            };

            // Act
            Func<Task> act = () => useCase.Create(request);

            await Assert.ThrowsAsync<DomainException>(act);
        }
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Create_ShouldThrowDomainException_WhenCapacityIsZeroOrNegative(int capacityInput)
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var useCase = new ResourceUseCase(context);

            var request = new CreateResource
            {
                Name = "test",
                Description = "test",
                Capacity = capacityInput,
                HourlyRate = 1.99m,
            };

            // Act
            Func<Task> act = () => useCase.Create(request);

            await Assert.ThrowsAsync<DomainException>(act);
        }
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Create_ShouldThrowDomainException_WhenHourlyRateIsZeroOrNegative(decimal hourlyRateInput)
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var useCase = new ResourceUseCase(context);

            var request = new CreateResource
            {
                Name = "test",
                Description = "test",
                Capacity = 1,
                HourlyRate = hourlyRateInput,
            };

            // Act
            Func<Task> act = () => useCase.Create(request);

            await Assert.ThrowsAsync<DomainException>(act);
        }
        [Fact]
        public async Task Update_ShouldUpdateResource_WhenRequestIsValid()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var existingResource = new Resource("Sala A", "Sala de juntas", 15, 120m);

            context.Resources.Add(existingResource);
            await context.SaveChangesAsync();

            var useCase = new ResourceUseCase(context);

            var request = new UpdateResource
            {
                Name = "test",
                Description = "test",
                Capacity = 1,
                HourlyRate = 1.99m,
            };

            // Act
            await useCase.Update(existingResource.Id, request);

            // Assert
            Assert.Equal(existingResource.Name, request.Name);
            Assert.Equal(existingResource.Description, request.Description);
            Assert.Equal(existingResource.Capacity, request.Capacity);
            Assert.Equal(existingResource.HourlyRate, request.HourlyRate);

            var reservationInDb = await context.Resources.FindAsync(existingResource.Id);
            Assert.NotNull(reservationInDb);
        }
        [Fact]
        public async Task Update_ShouldThrowNotFoundException_WhenResourceDoesNotExist()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var existingResource = new Resource("Sala A", "Sala de juntas", 15, 120m);

            await context.SaveChangesAsync();

            var useCase = new ResourceUseCase(context);

            var request = new UpdateResource
            {
                Name = "test",
                Description = "test",
                Capacity = 1,
                HourlyRate = 1.99m,
            };

            // Act
            Func<Task> act = () => useCase.Update(existingResource.Id, request);

            await Assert.ThrowsAsync<NotFoundException>(act);
        }
        [Fact]
        public async Task Update_ShouldThrowConflictException_WhenResourceNameAlreadyExists()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var existingResource = new Resource("Sala B", "Sala de juntas", 15, 120m);

            context.Resources.Add(resource);
            context.Resources.Add(existingResource);
            await context.SaveChangesAsync();

            var useCase = new ResourceUseCase(context);

            var request = new UpdateResource
            {
                Name = "Sala A",
                Description = "test",
                Capacity = 1,
                HourlyRate = 1.99m,
            };

            // Act
            Func<Task> act = () => useCase.Update(existingResource.Id, request);

            await Assert.ThrowsAsync<ConflictException>(act);
        }
        [Fact]
        public async Task Update_ShouldThrowDomainException_WhenNameIsEmpty()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var existingResource = new Resource("Sala B", "Sala de juntas", 15, 120m);

            context.Resources.Add(existingResource);
            await context.SaveChangesAsync();

            var useCase = new ResourceUseCase(context);

            var request = new UpdateResource
            {
                Name = "",
                Description = "test",
                Capacity = 1,
                HourlyRate = 1.99m,
            };

            // Act
            Func<Task> act = () => useCase.Update(existingResource.Id, request);

            await Assert.ThrowsAsync<DomainException>(act);
        }
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Update_ShouldThrowDomainException_WhenCapacityIsZeroOrNegative(int capacityInput)
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var existingResource = new Resource("Sala B", "Sala de juntas", 15, 120m);

            context.Resources.Add(existingResource);
            await context.SaveChangesAsync();

            var useCase = new ResourceUseCase(context);

            var request = new UpdateResource
            {
                Name = "test",
                Description = "test",
                Capacity = capacityInput,
                HourlyRate = 1.99m,
            };

            // Act
            Func<Task> act = () => useCase.Update(existingResource.Id, request);

            await Assert.ThrowsAsync<DomainException>(act);
        }
        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Update_ShouldThrowDomainException_WhenHourlyRateIsZeroOrNegative(decimal hourlyRateInput)
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var existingResource = new Resource("Sala B", "Sala de juntas", 15, 120m);

            context.Resources.Add(existingResource);
            await context.SaveChangesAsync();

            var useCase = new ResourceUseCase(context);

            var request = new UpdateResource
            {
                Name = "test",
                Description = "test",
                Capacity = 1,
                HourlyRate = hourlyRateInput,
            };

            // Act
            Func<Task> act = () => useCase.Update(existingResource.Id, request);

            await Assert.ThrowsAsync<DomainException>(act);
        }
        [Fact]
        public async Task Delete_ShouldDeleteResource_WhenResourceHasNoReservations()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var existingResource = new Resource("Sala A", "Sala de juntas", 15, 120m);

            context.Resources.Add(existingResource);
            await context.SaveChangesAsync();

            var useCase = new ResourceUseCase(context);

            // Act
            await useCase.Delete(existingResource.Id);

            // Assert
            var reservationInDb = await context.Resources.FindAsync(existingResource.Id);
            Assert.Null(reservationInDb);
        }
        [Fact]
        public async Task Delete_ShouldThrowNotFoundException_WhenResourceDoesNotExist()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var existingResource = new Resource("Sala A", "Sala de juntas", 15, 120m);

            await context.SaveChangesAsync();

            var useCase = new ResourceUseCase(context);

            // Act
            Func<Task> act = () => useCase.Delete(existingResource.Id);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }
        [Fact]
        public async Task Delete_ShouldThrowConflictException_WhenResourceHasReservations()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var existingResource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var existingReservation = new Reservation(customer.Id, existingResource.Id, startTime, endTime, 100m);

            context.Customers.Add(customer);
            context.Resources.Add(existingResource);
            context.Reservations.Add(existingReservation);

            await context.SaveChangesAsync();

            var useCase = new ResourceUseCase(context);

            // Act
            Func<Task> act = () => useCase.Delete(existingResource.Id);

            // Assert
            await Assert.ThrowsAsync<ConflictException>(act);
        }
        [Fact]
        public async Task Deactivate_ShouldDeactivateResource_WhenResourceExists()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var existingResource = new Resource("Sala A", "Sala de juntas", 15, 120m);

            context.Resources.Add(existingResource);
            await context.SaveChangesAsync();

            var useCase = new ResourceUseCase(context);

            // Act
            await useCase.Deactivate(existingResource.Id);

            // Assert
            Assert.False(existingResource.IsActive);

        }
        [Fact]
        public async Task Deactivate_ShouldThrowNotFoundException_WhenResourceDoesNotExist()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var existingResource = new Resource("Sala A", "Sala de juntas", 15, 120m);

            await context.SaveChangesAsync();

            var useCase = new ResourceUseCase(context);

            // Act
            Func<Task> act = () => useCase.Deactivate(existingResource.Id);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);

        }
        [Fact]
        public async Task Activate_ShouldActivateResource_WhenResourceExists()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var existingResource = new Resource("Sala A", "Sala de juntas", 15, 120m);

            context.Resources.Add(existingResource);
            await context.SaveChangesAsync();

            var useCase = new ResourceUseCase(context);

            // Act
            await useCase.Activate(existingResource.Id);

            // Assert
            Assert.True(existingResource.IsActive);

        }
        [Fact]
        public async Task Activate_ShouldThrowNotFoundException_WhenResourceDoesNotExist()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var existingResource = new Resource("Sala A", "Sala de juntas", 15, 120m);

            await context.SaveChangesAsync();

            var useCase = new ResourceUseCase(context);

            // Act
            Func<Task> act = () => useCase.Activate(existingResource.Id);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);

        }
    }
}
