using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using ReservationManagementApi_06.Infrastructure;

namespace ReservationManagementApi_06.Tests.TestSupport
{
    public sealed class TestDbContextFactory : IDisposable
    {
        private readonly SqliteConnection _connection;

        public AppDbContext Context { get; }

        public TestDbContextFactory()
        {
            _connection = new SqliteConnection("DataSource=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            Context = new AppDbContext(options);
            Context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            Context.Dispose();
            _connection.Dispose();
        }
    }
}
