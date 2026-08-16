using Microsoft.EntityFrameworkCore;
using ReservationManagementApi_06.Domain;

namespace ReservationManagementApi_06.Infrastructure
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Resource> Resources => Set<Resource>();
        public DbSet<Reservation> Reservations => Set<Reservation>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
    }
}
