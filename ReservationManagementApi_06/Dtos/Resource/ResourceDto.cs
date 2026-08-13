namespace ReservationManagementApi_06.Dtos.Resource
{
    public class ResourceDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Capacity { get; set; }
        public decimal HourlyRate { get; set; }
        public bool IsActive { get; set; }
    }
}
