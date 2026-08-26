using ReservationManagementApi_06.Domain;
using ReservationManagementApi_06.Dtos.Reservation;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace ReservationManagementApi_06.Tests.Integration;

public class ReservationEndpointsTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ReservationEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostReservation_ShouldCreateReservation_WhenRequestIsValid()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        Customer? customer = null;
        Resource? resource = null;

        await _factory.SeedAsync(async context =>
        {
            customer = new Customer("José Gallardo", "jose@test.com");
            resource = new Resource("Sala A", "Sala de juntas", 15, 120m);

            context.Customers.Add(customer);
            context.Resources.Add(resource);

            await context.SaveChangesAsync();
        });

        var startTime = DateTime.UtcNow.Date.AddDays(1).AddHours(10);
        var endTime = startTime.AddHours(2);

        var request = new CreateReservation
        {
            CustomerId = customer!.Id,
            ResourceId = resource!.Id,
            StartDateTime = startTime,
            EndDateTime = endTime
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/reservations", request);

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<ReservationDto>();

        Assert.NotNull(result);
        Assert.Equal(customer.Id, result.CustomerId);
        Assert.Equal(resource.Id, result.ResourceId);
        Assert.Equal(StatusReservation.Pending, result.Status);
        Assert.Equal(240m, result.TotalPrice);
    }
    [Fact]
    public async Task PostReservation_ShouldReturnNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        Resource? resource = null;

        await _factory.SeedAsync(async context =>
        {
            resource = new Resource("Sala A", "Sala de juntas", 15, 120m);

            context.Resources.Add(resource);

            await context.SaveChangesAsync();
        });

        var startTime = DateTime.UtcNow.Date.AddDays(1).AddHours(10);
        var endTime = startTime.AddHours(2);

        var request = new CreateReservation
        {
            CustomerId = Guid.NewGuid(),
            ResourceId = resource!.Id,
            StartDateTime = startTime,
            EndDateTime = endTime
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/reservations", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
    [Fact]
    public async Task PostReservation_ShouldReturnConflict_WhenReservationOverlaps()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        Customer? customer = null;
        Resource? resource = null;

        var startTime = DateTime.UtcNow.Date.AddDays(1).AddHours(10);
        var endTime = startTime.AddHours(2);

        await _factory.SeedAsync(async context =>
        {
            customer = new Customer("José Gallardo", "jose@test.com");
            resource = new Resource("Sala A", "Sala de juntas", 15, 120m);

            var existingReservation = new Reservation(
                customer.Id,
                resource.Id,
                startTime,
                endTime,
                240m);

            context.Customers.Add(customer);
            context.Resources.Add(resource);
            context.Reservations.Add(existingReservation);

            await context.SaveChangesAsync();
        });

        var request = new CreateReservation
        {
            CustomerId = customer!.Id,
            ResourceId = resource!.Id,
            StartDateTime = startTime.AddHours(1),
            EndDateTime = endTime.AddHours(1)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/reservations", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
   
}