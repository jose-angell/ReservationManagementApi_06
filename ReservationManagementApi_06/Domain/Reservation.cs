using ReservationManagementApi_06.Exceptions;

namespace ReservationManagementApi_06.Domain
{
    public class Reservation
    {
        public Guid Id { get; private set; }
        public Guid CustomerId { get; private set; }
        public Guid ResourceId { get; private set; }
        public DateTime StartDateTime { get; private set; }
        public DateTime EndDateTime { get; private set; }
        public StatusReservation Status { get; private set; }
        public decimal TotalPrice { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public Customer Customer { get; private set; } = null!;
        public Resource Resource { get; private set; } = null!;

        private Reservation() { }

        public Reservation(Guid customerId, Guid resourceId, DateTime startDateTime, DateTime endDateTime, decimal totalPrice)
        {
            ValidateIdEmpty(customerId, resourceId);
            ValidateDates(startDateTime, endDateTime);
            ValidateTotalPrice(totalPrice);

            Id = Guid.NewGuid();
            CustomerId = customerId;
            ResourceId = resourceId;
            StartDateTime = startDateTime.ToUniversalTime();
            EndDateTime = endDateTime.ToUniversalTime();
            Status = StatusReservation.Pending;
            TotalPrice = totalPrice;
            CreatedAt = DateTime.UtcNow;
        }
        public void Update(Guid resourceId, DateTime startDateTime, DateTime endDateTime, decimal totalPrice)
        {
            if (Status != StatusReservation.Pending) throw new DomainException("Solo las reservas penditentes pueden ser editadas");
            ValidateDates(startDateTime, endDateTime);
            ValidateTotalPrice(totalPrice);
            ResourceId = resourceId;
            StartDateTime = startDateTime.ToUniversalTime();
            EndDateTime = endDateTime.ToUniversalTime();
            TotalPrice = totalPrice;
        }
        public void UpdateDates(DateTime startDateTime, DateTime endDateTime, decimal totalPrice)
        {
            ValidateDates(startDateTime, endDateTime);
            StartDateTime = startDateTime.ToUniversalTime();
            EndDateTime = endDateTime.ToUniversalTime();
            TotalPrice = totalPrice;
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
            if (Status == StatusReservation.Cancelled)
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
        private void ValidateIdEmpty(Guid customerId, Guid resourceId)
        {
            if (customerId == Guid.Empty)
            {
                throw new DomainException("Customer cannot be null or empty.");
            }
            if (resourceId == Guid.Empty)
            {
                throw new DomainException("Resource cannot be null or empty.");
            }
        }
        private void ValidateDates(DateTime startDateTime, DateTime endDateTime)
        {
            if (startDateTime >= endDateTime)
            {
                throw new DomainException("Start date and time must be before end date and time.");
            }

            if (startDateTime < DateTime.UtcNow)
            {
                throw new DomainException("la fecha no puede estar en el pasado.");
            }
        }
        private void ValidateTotalPrice(decimal totalPrice)
        {
            if (totalPrice <= 0)
            {
                throw new DomainException("Total price must be a positive value.");
            }
        }
    }
}
