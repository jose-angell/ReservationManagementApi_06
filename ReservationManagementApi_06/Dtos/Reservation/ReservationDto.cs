using ReservationManagementApi_06.Domain;

namespace ReservationManagementApi_06.Dtos.Reservation
{
    public class ReservationDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public Guid ResourceId { get; set; }
        public string ResourceName { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public StatusReservation Status { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
