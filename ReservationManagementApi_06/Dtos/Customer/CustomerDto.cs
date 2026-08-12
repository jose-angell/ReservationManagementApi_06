namespace ReservationManagementApi_06.Dtos.Customer
{
    public class CustomerDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

    }
}
