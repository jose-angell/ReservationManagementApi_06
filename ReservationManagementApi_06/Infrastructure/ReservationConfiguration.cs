using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservationManagementApi_06.Domain;

namespace ReservationManagementApi_06.Infrastructure
{
    public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
    {
        public void Configure(EntityTypeBuilder<Reservation> builder)
        {
            builder.ToTable("reservations");
            builder.HasKey(r => r.Id);
            builder.Property(r => r.CustomerId).IsRequired();
            builder.Property(r => r.ResourceId).IsRequired();
            builder.Property(r => r.StartDateTime).IsRequired();
            builder.Property(r =>r.EndDateTime).IsRequired();
            builder.Property(r => r.Status).IsRequired();
            builder.Property(r => r.TotalPrice).HasColumnName("total_price").HasPrecision(18, 2).IsRequired();
            builder.Property(r => r.CreatedAt).IsRequired();

            builder.HasOne(r => r.Customer).WithMany(r => r.Reservations).HasForeignKey(r => r.CustomerId);
            builder.HasOne(r => r.Resource).WithMany(r => r.Reservations).HasForeignKey(r => r.ResourceId);
        }
    }
}
