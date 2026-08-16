using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservationManagementApi_06.Domain;

namespace ReservationManagementApi_06.Infrastructure
{
    public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
    {
        public void Configure(EntityTypeBuilder<Resource> builder)
        {
            builder.ToTable("resources");
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Name).IsRequired().HasMaxLength(300);
            builder.Property(r => r.Description).HasMaxLength(2000);
            builder.Property(r => r.Capacity).IsRequired();
            builder.Property(r => r.HourlyRate).IsRequired();
            builder.Property(r => r.IsActive).IsRequired();

        }
    }
}
