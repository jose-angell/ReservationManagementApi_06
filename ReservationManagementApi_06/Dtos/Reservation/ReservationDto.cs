using ReservationManagementApi_06.Domain;

namespace ReservationManagementApi_06.Dtos.Reservation
{
    public class ReservationDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid ResourceId { get; set; }
        public DateTimeOffset StartDateTime { get; set; }
        public DateTimeOffset EndDateTime { get; set; }
        public StatusReservation Status { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
