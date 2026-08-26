using ReservationManagementApi_06.Application;
using ReservationManagementApi_06.Domain;
using ReservationManagementApi_06.Dtos.Customer;
using ReservationManagementApi_06.Dtos.Reservation;
using ReservationManagementApi_06.Exceptions;
using ReservationManagementApi_06.Tests.TestSupport;

namespace ReservationManagementApi_06.Tests.Application
{
    public class CustomerUseCaseTests
    {
        [Fact]
        public async Task Create_ShouldCreateCustomer_WhenRequestIsValid()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var useCase = new CustomerUseCase(context);

            var request = new CreateCustomer
            {
                FullName = "test",
                Email = "test@gmail.com"
            };

            // Act
            var result = await useCase.Create(request);

            // Assert
            Assert.Equal(request.FullName, result.FullName);
            Assert.Equal(request.Email, result.Email);

            var reservationInDb = await context.Customers.FindAsync(result.Id);
            Assert.NotNull(reservationInDb);
        }
        [Fact]
        public async Task Create_ShouldThrowConflictException_WhenEmailAlreadyExists()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var customer = new Customer("test", "test@gmail.com");

            context.Customers.Add(customer);

            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new CustomerUseCase(context);

            var request = new CreateCustomer
            {
                FullName = "test",
                Email = "test@gmail.com"
            };

            // Act
            Func<Task> act = () =>  useCase.Create(request);

            await Assert.ThrowsAsync<ConflictException>(act);
        }
        [Fact]
        public async Task Create_ShouldThrowDomainException_WhenFullNameIsEmpty()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var useCase = new CustomerUseCase(context);

            var request = new CreateCustomer
            {
                FullName = "",
                Email = "test@gmail.com"
            };

            // Act
            Func<Task> act = () => useCase.Create(request);

            await Assert.ThrowsAsync<DomainException>(act);
        }
        [Fact]
        public async Task Create_ShouldThrowDomainException_WhenEmailIsInvalid()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var useCase = new CustomerUseCase(context);

            var request = new CreateCustomer
            {
                FullName = "test",
                Email = ""
            };

            // Act
            Func<Task> act = () => useCase.Create(request);

            await Assert.ThrowsAsync<DomainException>(act);
        }
        [Fact]
        public async Task Update_ShouldUpdateCustomer_WhenRequestIsValid()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var customer = new Customer("test", "test@gmail.com");

            context.Customers.Add(customer);

            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new CustomerUseCase(context);

            var request = new UpdateCustomer
            {
                FullName = "test",
                Email = "test@gmail.com"
            };

            // Act
            await useCase.Update(customer.Id,request);

            // Assert
            Assert.Equal(customer.FullName, request.FullName);
            Assert.Equal(customer.Email, request.Email);

        }
        [Fact]
        public async Task Update_ShouldThrowConflictException_WhenEmailAlreadyExists()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var customer = new Customer("test", "test@gmail.com");
            var Existingcustomer = new Customer("test", "testnew@gmail.com");

            context.Customers.Add(customer);
            context.Customers.Add(Existingcustomer);

            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new CustomerUseCase(context);

            var request = new UpdateCustomer
            {
                FullName = "test",
                Email = "test@gmail.com"
            };

            // Act
            Func<Task> act = () => useCase.Update(Existingcustomer.Id, request);

            await Assert.ThrowsAsync<ConflictException>(act);
        }
        [Fact]
        public async Task Update_ShouldThrowNotFoundException_WhenCustomerDoesNotExist()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;
            var customer = new Customer("test", "test@gmail.com");


            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new CustomerUseCase(context);

            var request = new UpdateCustomer
            {
                FullName = "test",
                Email = "test@gmail.com"
            };

            // Act
            Func<Task> act = () => useCase.Update(customer.Id, request);

            await Assert.ThrowsAsync<NotFoundException>(act);
        }
        [Fact]
        public async Task GetById_ShouldReturnCustomer_WhenCustomerExists()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var customer = new Customer("test", "test@gmail.com");

            context.Customers.Add(customer);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new CustomerUseCase(context);

            // Act
            var result = await useCase.GetById(customer.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customer.Id, result.Id);
            Assert.Equal(customer.FullName, result.FullName);
            Assert.Equal(customer.Email, result.Email);
            Assert.Equal(customer.CreatedAt, result.CreatedAt);
        }
        [Fact]
        public async Task GetById_ShouldThrowNotFoundException_WhenCustomerDoesNotExist()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var customer = new Customer("test", "test@gmail.com");


            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new CustomerUseCase(context);

            // Act
            Func<Task> act = () => useCase.GetById(customer.Id);

            // Assert
            await Assert.ThrowsAsync<NotFoundException>(act);
        }
        [Fact]
        public async Task GetAll_ShouldReturnAllCustomers_WhenNoFiltersAreProvided()
        {
            // Arrange
            using var db = new TestDbContextFactory();
            var context = db.Context;

            var startTime = DateTime.UtcNow.AddHours(1);
            var endTime = startTime.AddHours(2);

            var customerOne = new Customer("CutomerOne", "one@test.com");
            var customerTwo = new Customer("CustomerTwo", "two@test.com");


            context.Customers.Add(customerOne);
            context.Customers.Add(customerTwo);
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);

            var useCase = new CustomerUseCase(context);

            // Act
            var result = await useCase.GetAll();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }
    }
}
