using ReservationManagementApi_06.Domain;

namespace ReservationManagementApi_06.Dtos.Reservation
{
    public class ReservationQuery
    {
        public Guid? CustomerId { get; set; }
        public Guid? ResourceId { get; set; }
        public StatusReservation? Status {  get; set; }
        public DateTimeOffset? FromDate { get; set; }
        public DateTimeOffset? ToDate { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? SortBy { get; set; }
    }
}
