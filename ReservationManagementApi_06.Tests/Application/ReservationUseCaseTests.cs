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
        [Fact]
        public async Task Update_ShouldUpdateReservation_WhenRequestIsValid()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var newResource = new Resource("Sala B", "Sala de juntas B", 10, 100m);
            var pendingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);
            

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Resources.Add(newResource);
            context.Reservations.Add(pendingReservation);
            await context.SaveChangesAsync();

            var useCase = new ReservationUseCase(context);


            var newStartTime = startTime.AddHours(3);
            var newEndTime = endTime.AddHours(4);

            var request = new UpdateReservation
            {
                ResourceId = newResource.Id,
                StartDateTime = newStartTime,
                EndDateTime = newEndTime
            };

            // Act
            await useCase.Update(pendingReservation.Id,request);

            // Assert
            Assert.Equal(customer.Id, pendingReservation.CustomerId);
            Assert.Equal(newResource.Id, pendingReservation.ResourceId);
            Assert.Equal(StatusReservation.Pending, pendingReservation.Status);
            Assert.Equal(300m, pendingReservation.TotalPrice);

            var reservationInDb = await context.Reservations.FindAsync(pendingReservation.Id);
            Assert.NotNull(reservationInDb);
        }
        [Fact]
        public async Task Update_ShouldThrowNotFoundException_WhenReservationDoesNotExist()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var newResource = new Resource("Sala B", "Sala de juntas B", 10, 100m);
            var pendingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);


            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Resources.Add(newResource);
            await context.SaveChangesAsync();

            var useCase = new ReservationUseCase(context);


            var newStartTime = startTime.AddHours(3);
            var newEndTime = endTime.AddHours(4);
            var request = new UpdateReservation
            {
                ResourceId = newResource.Id,
                StartDateTime = newStartTime,
                EndDateTime = newEndTime
            };

            // Act
            Func<Task> act = () => useCase.Update(pendingReservation.Id, request);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }
        [Fact]
        public async Task Update_ShouldThrowConflictException_WhenReservationIsConfirmed()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var newResource = new Resource("Sala B", "Sala de juntas B", 10, 100m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);
            existingReservation.Confirm();

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Resources.Add(newResource);
            context.Reservations.Add(existingReservation);
            await context.SaveChangesAsync();

            var useCase = new ReservationUseCase(context);


            var newStartTime = startTime.AddHours(3);
            var newEndTime = endTime.AddHours(4);
            var request = new UpdateReservation
            {
                ResourceId = newResource.Id,
                StartDateTime = newStartTime,
                EndDateTime = newEndTime
            };

            // Act
            Func<Task> act = () => useCase.Update(existingReservation.Id, request);

            // Assert
            await Assert.ThrowsAsync<ConflictException>(act);
        }
        [Fact]
        public async Task Update_ShouldThrowConflictException_WhenReservationIsCancelled()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var newResource = new Resource("Sala B", "Sala de juntas B", 10, 100m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);
            existingReservation.Cancel();

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Resources.Add(newResource);
            context.Reservations.Add(existingReservation);
            await context.SaveChangesAsync();

            var useCase = new ReservationUseCase(context);


            var newStartTime = startTime.AddHours(3);
            var newEndTime = endTime.AddHours(4);
            var request = new UpdateReservation
            {
                ResourceId = newResource.Id,
                StartDateTime = newStartTime,
                EndDateTime = newEndTime
            };

            // Act
            Func<Task> act = () => useCase.Update(existingReservation.Id, request);

            // Assert
            await Assert.ThrowsAsync<ConflictException>(act);
        }
        [Fact]
        public async Task Update_ShouldThrowConflictException_WhenReservationIsCompleted()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var newResource = new Resource("Sala B", "Sala de juntas B", 10, 100m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);
            existingReservation.Confirm();
            existingReservation.Complete();

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Resources.Add(newResource);
            context.Reservations.Add(existingReservation);
            await context.SaveChangesAsync();

            var useCase = new ReservationUseCase(context);


            var newStartTime = startTime.AddHours(3);
            var newEndTime = endTime.AddHours(4);
            var request = new UpdateReservation
            {
                ResourceId = newResource.Id,
                StartDateTime = newStartTime,
                EndDateTime = newEndTime
            };

            // Act
            Func<Task> act = () => useCase.Update(existingReservation.Id, request);

            // Assert
            await Assert.ThrowsAsync<ConflictException>(act);
        }
        [Fact]
        public async Task Update_ShouldThrowNotFoundException_WhenResourceDoesNotExist()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Reservations.Add(existingReservation);
            await context.SaveChangesAsync();

            var useCase = new ReservationUseCase(context);


            var newStartTime = startTime.AddHours(3);
            var newEndTime = endTime.AddHours(4);
            var request = new UpdateReservation
            {
                ResourceId = Guid.NewGuid(),
                StartDateTime = newStartTime,
                EndDateTime = newEndTime
            };

            // Act
            Func<Task> act = () => useCase.Update(existingReservation.Id, request);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }
        [Fact]
        public async Task Update_ShouldThrowConflictException_WhenResourceIsInactive()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var newResource = new Resource("Sala B", "Sala de juntas B", 10, 100m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);
            newResource.Deactivate();

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Resources.Add(newResource);
            context.Reservations.Add(existingReservation);
            await context.SaveChangesAsync();

            var useCase = new ReservationUseCase(context);


            var newStartTime = startTime.AddHours(3);
            var newEndTime = endTime.AddHours(4);
            var request = new UpdateReservation
            {
                ResourceId = newResource.Id,
                StartDateTime = newStartTime,
                EndDateTime = newEndTime
            };

            // Act
            Func<Task> act = () => useCase.Update(existingReservation.Id, request);

            // Assert
            await Assert.ThrowsAsync<ConflictException>(act);
        }
        [Fact]
        public async Task Update_ShouldThrowConflictException_WhenUpdatedDatesOverlapPendingReservation()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var newResource = new Resource("Sala B", "Sala de juntas B", 10, 100m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);
            var oldReservation = new Reservation(customer.Id, newResource.Id, startTime, endTime, 100m);

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Resources.Add(newResource);
            context.Reservations.Add(oldReservation);
            context.Reservations.Add(existingReservation);
            await context.SaveChangesAsync();

            var useCase = new ReservationUseCase(context);


            var newStartTime = startTime;
            var newEndTime = endTime;
            var request = new UpdateReservation
            {
                ResourceId = newResource.Id,
                StartDateTime = newStartTime,
                EndDateTime = newEndTime
            };

            // Act
            Func<Task> act = () => useCase.Update(existingReservation.Id, request);

            // Assert
            await Assert.ThrowsAsync<ConflictException>(act);
        }
        [Fact]
        public async Task Update_ShouldThrowConflictException_WhenUpdatedDatesOverlapConfirmedReservation()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var newResource = new Resource("Sala B", "Sala de juntas B", 10, 100m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);
            var oldReservation = new Reservation(customer.Id, newResource.Id, startTime, endTime, 100m);
            oldReservation.Confirm();

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Resources.Add(newResource);
            context.Reservations.Add(oldReservation);
            context.Reservations.Add(existingReservation);
            await context.SaveChangesAsync();

            var useCase = new ReservationUseCase(context);


            var newStartTime = startTime;
            var newEndTime = endTime;
            var request = new UpdateReservation
            {
                ResourceId = newResource.Id,
                StartDateTime = newStartTime,
                EndDateTime = newEndTime
            };

            // Act
            Func<Task> act = () => useCase.Update(existingReservation.Id, request);

            // Assert
            await Assert.ThrowsAsync<ConflictException>(act);
        }
        [Fact]
        public async Task Update_ShouldUpdateReservation_WhenOverlappingReservationIsCancelled()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var newResource = new Resource("Sala B", "Sala de juntas B", 10, 100m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);
            var oldReservation = new Reservation(customer.Id, newResource.Id, startTime, endTime, 100m);
            oldReservation.Cancel();

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Resources.Add(newResource);
            context.Reservations.Add(oldReservation);
            context.Reservations.Add(existingReservation);
            await context.SaveChangesAsync();

            var useCase = new ReservationUseCase(context);


            var newStartTime = startTime;
            var newEndTime = endTime;
            var request = new UpdateReservation
            {
                ResourceId = newResource.Id,
                StartDateTime = newStartTime,
                EndDateTime = newEndTime
            };

            // Act
            await useCase.Update(existingReservation.Id, request);

            // Assert
            Assert.Equal(customer.Id, existingReservation.CustomerId);
            Assert.Equal(newResource.Id, existingReservation.ResourceId);
            Assert.Equal(StatusReservation.Pending, existingReservation.Status);
            Assert.Equal(200m, existingReservation.TotalPrice);

            var reservationInDb = await context.Reservations.FindAsync(existingReservation.Id);
            Assert.NotNull(reservationInDb);
        }
        [Fact]
        public async Task Update_ShouldUpdateReservation_WhenOverlappingReservationIsCompleted()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var newResource = new Resource("Sala B", "Sala de juntas B", 10, 100m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);
            var oldReservation = new Reservation(customer.Id, newResource.Id, startTime, endTime, 100m);
            oldReservation.Confirm();
            oldReservation.Complete();

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Resources.Add(newResource);
            context.Reservations.Add(oldReservation);
            context.Reservations.Add(existingReservation);
            await context.SaveChangesAsync();

            var useCase = new ReservationUseCase(context);


            var newStartTime = startTime;
            var newEndTime = endTime;
            var request = new UpdateReservation
            {
                ResourceId = newResource.Id,
                StartDateTime = newStartTime,
                EndDateTime = newEndTime
            };

            // Act
            await useCase.Update(existingReservation.Id, request);

            // Assert
            Assert.Equal(customer.Id, existingReservation.CustomerId);
            Assert.Equal(newResource.Id, existingReservation.ResourceId);
            Assert.Equal(StatusReservation.Pending, existingReservation.Status);
            Assert.Equal(200m, existingReservation.TotalPrice);

            var reservationInDb = await context.Reservations.FindAsync(existingReservation.Id);
            Assert.NotNull(reservationInDb);
        }
        [Fact]
        public async Task Delete_ShouldDeleteReservation_WhenReservationIsPending()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Reservations.Add(existingReservation);
            await context.SaveChangesAsync();

            var useCase = new ReservationUseCase(context);

            // Act
            await useCase.Delete(existingReservation.Id);

            // Assert
            var reservationInDb = await context.Reservations.FindAsync(existingReservation.Id);
            Assert.Null(reservationInDb);
        }
        [Fact]
        public async Task Delete_ShouldThrowNotFoundException_WhenReservationDoesNotExist()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            await context.SaveChangesAsync();

            var useCase = new ReservationUseCase(context);

            // Act
            Func<Task> act = () => useCase.Delete(existingReservation.Id);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }
        [Fact]
        public async Task Delete_ShouldThrowConflictException_WhenReservationIsConfirmed()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);
            
            existingReservation.Confirm();

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Reservations.Add(existingReservation);
            await context.SaveChangesAsync();

            var useCase = new ReservationUseCase(context);

            // Act
            Func<Task> act = () => useCase.Delete(existingReservation.Id);

            // Assert
            await Assert.ThrowsAsync<ConflictException>(act);
        }
        [Fact]
        public async Task Delete_ShouldThrowConflictException_WhenReservationIsCancelled()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);
            
            existingReservation.Cancel();

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Reservations.Add(existingReservation);
            await context.SaveChangesAsync();

            var useCase = new ReservationUseCase(context);

            // Act
            Func<Task> act = () => useCase.Delete(existingReservation.Id);

            // Assert
            await Assert.ThrowsAsync<ConflictException>(act);
        }
        [Fact]
        public async Task Delete_ShouldThrowConflictException_WhenReservationIsCompleted()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);
           
            existingReservation.Confirm();
            existingReservation.Complete();

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Reservations.Add(existingReservation);
            await context.SaveChangesAsync();

            var useCase = new ReservationUseCase(context);

            // Act
            Func<Task> act = () => useCase.Delete(existingReservation.Id);

            // Assert
            await Assert.ThrowsAsync<ConflictException>(act);
        }
        [Fact]
        public async Task Confirm_ShouldConfirmReservation_WhenReservationIsPending()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);


            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Reservations.Add(existingReservation);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new ReservationUseCase(context);

            // Act
            await useCase.Confirm(existingReservation.Id);

            // Assert
            Assert.Equal(StatusReservation.Confirmed, existingReservation.Status);

            var reservationInDb = await context.Reservations.FindAsync(existingReservation.Id);
            Assert.NotNull(reservationInDb);
        }
        [Fact]
        public async Task Confirm_ShouldThrowNotFoundException_WhenReservationDoesNotExist()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);


            context.Customers.Add(customer);
            context.Resources.Add(resource);
           
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new ReservationUseCase(context);

            // Act
            Func<Task> act = () => useCase.Confirm(existingReservation.Id);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }
        [Fact]
        public async Task Confirm_ShouldThrowDomainException_WhenReservationIsCancelled()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);

            existingReservation.Cancel();
            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Reservations.Add(existingReservation);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new ReservationUseCase(context);

            // Act
            Func<Task> act = () => useCase.Confirm(existingReservation.Id);

            // Assert
            await Assert.ThrowsAsync<DomainException>(act);
        }
        [Fact]
        public async Task Confirm_ShouldThrowDomainException_WhenReservationIsCompleted()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);

            existingReservation.Confirm();
            existingReservation.Complete();
            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Reservations.Add(existingReservation);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new ReservationUseCase(context);

            // Act
            Func<Task> act = () => useCase.Confirm(existingReservation.Id);

            // Assert
            await Assert.ThrowsAsync<DomainException>(act);
        }
        [Fact]
        public async Task Cancel_ShouldCancelReservation_WhenReservationIsPending()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);


            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Reservations.Add(existingReservation);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new ReservationUseCase(context);

            // Act
            await useCase.Cancel(existingReservation.Id);

            // Assert
            Assert.Equal(StatusReservation.Cancelled, existingReservation.Status);

            var reservationInDb = await context.Reservations.FindAsync(existingReservation.Id);
            Assert.NotNull(reservationInDb);
        }
        [Fact]
        public async Task Cancel_ShouldCancelReservation_WhenReservationIsConfirmed()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);
            existingReservation.Confirm();

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Reservations.Add(existingReservation);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new ReservationUseCase(context);

            // Act
            await useCase.Cancel(existingReservation.Id);

            // Assert
            Assert.Equal(StatusReservation.Cancelled, existingReservation.Status);

            var reservationInDb = await context.Reservations.FindAsync(existingReservation.Id);
            Assert.NotNull(reservationInDb);
        }
        [Fact]
        public async Task Cancel_ShouldThrowNotFoundException_WhenReservationDoesNotExist()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new ReservationUseCase(context);

            // Act
            Func < Task > act = () => useCase.Cancel(existingReservation.Id);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }
        [Fact]
        public async Task Cancel_ShouldThrowDomainException_WhenReservationIsCompleted()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);

            existingReservation.Confirm();
            existingReservation.Complete();

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Reservations.Add(existingReservation);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new ReservationUseCase(context);

            // Act
            Func<Task> act = () => useCase.Cancel(existingReservation.Id);

            // Assert
            await Assert.ThrowsAsync<DomainException>(act);
        }
        [Fact]
        public async Task Complete_ShouldCompleteReservation_WhenReservationIsConfirmed()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);

            existingReservation.Confirm();

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Reservations.Add(existingReservation);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new ReservationUseCase(context);

            // Act
            await useCase.Complete(existingReservation.Id);

            // Assert
            Assert.Equal(StatusReservation.Completed, existingReservation.Status);

            var reservationInDb = await context.Reservations.FindAsync(existingReservation.Id);
            Assert.NotNull(reservationInDb);
        }
        [Fact]
        public async Task Complete_ShouldThrowNotFoundException_WhenReservationDoesNotExist()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);

            existingReservation.Confirm();

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new ReservationUseCase(context);

            // Act
            Func<Task> act = () => useCase.Complete(existingReservation.Id);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }
        [Fact]
        public async Task Complete_ShouldThrowDomainException_WhenReservationIsPending()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Reservations.Add(existingReservation);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new ReservationUseCase(context);

            // Act
            Func<Task> act = () => useCase.Complete(existingReservation.Id);

            // Assert
            await Assert.ThrowsAsync<DomainException>(act);
        }
        [Fact]
        public async Task Complete_ShouldThrowDomainException_WhenReservationIsCancelled()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("José Gallardo", "jose@test.com");
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            var existingReservation = new Reservation(customer.Id, resource.Id, startTime, endTime, 100m);

            existingReservation.Cancel();

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Reservations.Add(existingReservation);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new ReservationUseCase(context);

            // Act
            Func<Task> act = () => useCase.Complete(existingReservation.Id);

            // Assert
            await Assert.ThrowsAsync<DomainException>(act);
        }
    }
}
