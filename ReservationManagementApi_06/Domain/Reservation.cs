using ReservationManagementApi_06.Exceptions;

namespace ReservationManagementApi_06.Domain
{
    public class Reservation
    {
        public Guid Id { get; private set; }
        public Guid CustomerId { get; private set; } 
        public Guid ResourceId { get; private set; }
        public DateTimeOffset StartDateTime { get; private set; }
        public DateTimeOffset EndDateTime { get; private set; }
        public StatusReservation Status { get; private set; }
        public decimal TotalPrice { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        public Customer Customer { get; private set; } = null!;
        public Resource Resource { get; private set; } = null!;

        private Reservation() { }

        public Reservation(Guid customerId, Guid resourceId, DateTimeOffset startDateTime, DateTimeOffset endDateTime, decimal totalPrice)
        {
            ValidateDates(startDateTime, endDateTime);
            ValidateTotalPrice(totalPrice);

            Id = Guid.NewGuid();
            CustomerId = customerId;
            ResourceId = resourceId;
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
            Status = StatusReservation.Pending;
            TotalPrice = totalPrice;
            CreatedAt = DateTimeOffset.Now;
        }
        public void Update(Guid resourceId,DateTimeOffset startDateTime, DateTimeOffset endDateTime, decimal totalPrice)
        {
            ValidateDates(startDateTime, endDateTime);
            ValidateTotalPrice(totalPrice);
            ResourceId = resourceId;
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
            TotalPrice = totalPrice;
        }
        public void UpdateDates(DateTimeOffset startDateTime, DateTimeOffset endDateTime)
        {
            ValidateDates(startDateTime, endDateTime);
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
        }
        public void Confirm()
        {
            if (Status != StatusReservation.Pending)
            {
                throw new DomainException("Only pending reservations can be confirmed.");
            }
            Status = StatusReservation.Confirmed;
        }
        public void Cancel()
        {
            if (Status == StatusReservation.Cancelled )
            {
                throw new DomainException("Reservation is already cancelled.");
            }
            if (Status == StatusReservation.Completed)
            {
                throw new DomainException("Reservation is already completed.");
            }
            Status = StatusReservation.Cancelled;
        }
        public void Complete()
        {
            if (Status != StatusReservation.Confirmed)
            {
                throw new DomainException("Only confirmed reservations can be completed.");
            }
            Status = StatusReservation.Completed;
        }
        private void ValidateDates(DateTimeOffset startDateTime, DateTimeOffset endDateTime)
        {
            if (startDateTime >= endDateTime)
            {
                throw new DomainException("Start date and time must be before end date and time.");
            }
           
        }
        private void ValidateTotalPrice(decimal totalPrice)
        {
            if (totalPrice < 0)
            {
                throw new DomainException("Total price must be a positive value.");
            }
        }
    }
}
