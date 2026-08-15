using ReservationManagementApi_06.Dtos.Reservation;

namespace ReservationManagementApi_06.Dtos.Resource
{
    public class AvailabilityDto
    {
        public Guid ResourceId { get; set; }
        public string ResourceName { get; set; }
        public DateTimeOffset StartDateTime { get; set; }
        public DateTimeOffset EndDateTime { get; set; }
        public bool IsAvailable { get; set; }
        public IEnumerable<ReservationDto> conflictingReservations { get; set; } = new List<ReservationDto>();
    }
}
