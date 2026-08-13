using System.ComponentModel.DataAnnotations;

namespace ReservationManagementApi_06.Dtos.Resource
{
    public class CreateResource
    {
        [Required(ErrorMessage = "El nombre del recurso es obligatorio.")]
        [MaxLength(300, ErrorMessage = "El nombre debe tener como maximo 300 caractires.")]
        public string? Name { get; set; }

        [MaxLength(300, ErrorMessage = "El nombre debe tener como maximo 300 caractires.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "La capacidad es obligatoria")]
        [Range(1, 10000, ErrorMessage = "La capacidad no puede ser negativa y el máximo permitido es 10,000.")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "El precio de la tarifa es obligatoria")]
        [Range(0.01, 999999.99, ErrorMessage = "El precio de la tarifa debe ser mayor a 0 y menor a 1,000,000.")]
        public decimal HourlyRate { get; set; }
    }
}
