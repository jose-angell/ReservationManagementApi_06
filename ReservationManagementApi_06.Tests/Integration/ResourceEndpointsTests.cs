using ReservationManagementApi_06.Domain;
using ReservationManagementApi_06.Dtos.Reservation;
using ReservationManagementApi_06.Dtos.Resource;
using System.Net;
using System.Net.Http.Json;

namespace ReservationManagementApi_06.Tests.Integration;

public class ResourceEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public ResourceEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
    [Fact]
    public async Task PostResource_ShouldCreateResoruce_WhenRequestIsValid()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();
        var request = new CreateResource
        {
            Name = "Test 1",
            Description = "Tests description",
            Capacity = 100,
            HourlyRate = 1200.99m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/resources", request);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<ResourceDto>();

        Assert.NotNull(result);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Description, result.Description);
        Assert.Equal(request.Capacity, result.Capacity);
        Assert.Equal(request.HourlyRate, result.HourlyRate);
    }
    [Fact]
    public async Task PostResource_ShouldReturnConflict_WhenNameIsAreadyExists()
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

        var request = new CreateResource
        {
            Name = "Sala A",
            Description = "Tests description",
            Capacity = 100,
            HourlyRate = 1200.99m
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/resources", request);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.Conflict);
    }
    
    [Fact]
    public async Task PatchActiveResource_ShouldReturnNoContent_WhenResourceChangeToActive()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        Guid resourceId = Guid.Empty;

        await _factory.SeedAsync(async context =>
        {
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            resource.Deactivate();
            context.Resources.Add(resource);

            await context.SaveChangesAsync();

            resourceId = resource.Id;
        });

        // Act
        var response = await _client.PatchAsync($"/api/resources/activate/{resourceId}", content: null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
    [Fact]
    public async Task PatchDeactiveResource_ShouldReturnNoContent_WhenResourceChangeToDeactive()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();

        Guid resourceId = Guid.Empty;

        await _factory.SeedAsync(async context =>
        {
            var resource = new Resource("Sala A", "Sala de juntas", 15, 120m);
            context.Resources.Add(resource);

            await context.SaveChangesAsync();

            resourceId = resource.Id;
        });

        // Act
        var response = await _client.PatchAsync($"/api/resources/deactivate/{resourceId}", content: null);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }
}

