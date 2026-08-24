using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using ReservationManagementApi_06.Application;
using ReservationManagementApi_06.Domain;
using ReservationManagementApi_06.Dtos.Reservation;
using ReservationManagementApi_06.Tests.TestSupport;
using System;
using System.Collections.Generic;
using System.Text;

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
            var result = await useCase.Availability(resource.Id,startTime, endTime);

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
    }
}
