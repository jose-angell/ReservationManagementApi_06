using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReservationManagementApi_06.Domain;

namespace ReservationManagementApi_06.Infrastructure
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            builder.ToTable("customers");
            builder.HasKey(c => c.Id);
            builder.Property(c => c.FullName).IsRequired().HasMaxLength(300);
            builder.Property(c => c.Email).IsRequired().HasMaxLength(200);
            builder.Property(c => c.CreatedAt)
                    .HasColumnType("timestamptz")
                    .HasDefaultValueSql("now()").IsRequired();
            builder.HasIndex(c => c.Email).IsUnique();
        }
    }
}
