namespace ReservationManagementApi_06.Dtos.Resource
{
    public class ResourceQuery
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public int? MinCapacity { get; set; }
        public int? MaxCapacity { get; set; }
        public decimal? MinHourlyRate { get; set; }
        public decimal? MaxHourlyRate { get; set; }
        public bool? IsActive { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
