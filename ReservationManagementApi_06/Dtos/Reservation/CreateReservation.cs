using System.ComponentModel.DataAnnotations;

namespace ReservationManagementApi_06.Dtos.Reservation
{
    public class CreateReservation
    {
        [Required(ErrorMessage = "El cliente es obligatorio.")]
        public Guid? CustomerId { get; set; }

        [Required(ErrorMessage = "El recurso es obligatorio.")]
        public Guid? ResourceId { get; set; }

        [Required(ErrorMessage = "La fecha y horario de inicio es obligatorio.")]
        public DateTimeOffset? StartDateTime { get; set; }

        [Required(ErrorMessage = "La fecha y horario de fin es obligatorio.")]
        public DateTimeOffset? EndDateTime { get; set; }

    }
}
