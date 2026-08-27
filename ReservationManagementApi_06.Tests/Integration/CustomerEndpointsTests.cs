using ReservationManagementApi_06.Domain;
using ReservationManagementApi_06.Dtos.Customer;
using ReservationManagementApi_06.Dtos.Resource;
using System.Net;
using System.Net.Http.Json;

namespace ReservationManagementApi_06.Tests.Integration;

public class CustomerEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public CustomerEndpointsTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }
    [Fact]
    public async Task PostCustomer_ShouldCreateResoruce_WhenRequestIsValid()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();
        var request = new CreateCustomer
        {
            FullName = "Test 1",
            Email = "test@email.com",
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", request);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CustomerDto>();

        Assert.NotNull(result);
        Assert.Equal(request.FullName, result.FullName);
        Assert.Equal(request.Email, result.Email);
    }

    [Fact]
    public async Task PostCustomer_ShouldReturnConflict_WhenEmailIsAlreadyExists()
    {
        // Arrange
        await _factory.ResetDatabaseAsync();
        await _factory.SeedAsync(async context =>
        {
            var startTime = DateTime.UtcNow.Date.AddDays(1).AddHours(10);
            var endTime = startTime.AddHours(2);

            var customer = new Customer("Jos", "test@test.com");

            context.Customers.Add(customer);

            await context.SaveChangesAsync();

        });

        var request = new CreateCustomer
        {
            FullName = "Test 1",
            Email = "test@email.com",
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", request);

        // Assert
        Assert.True(response.StatusCode == HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<CustomerDto>();

    }
}

