using ReservationManagementApi_06.Exceptions;

namespace ReservationManagementApi_06.Domain
{
    public class Customer
    {
        public Guid Id { get; private set; }
        public string FullName { get; private set; }
        public string Email { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }

        
        private Customer()
        {
            FullName = string.Empty;
            Email = string.Empty;
            CreatedAt = DateTimeOffset.Now;
        }
        public Customer(string fullName, string email)
        {
            Validate(fullName, email);
            Id = Guid.NewGuid();
            FullName = fullName;
            Email = email;
            CreatedAt = DateTimeOffset.Now;
        }
        public void Update(string fullName, string email)
        {
            Validate(fullName, email);
            FullName = fullName;
            Email = email;
        }
        private void Validate(string fullName, string email)
        {
            if (string.IsNullOrWhiteSpace(fullName))
            {
                throw new DomainException("fullName cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new DomainException("email cannot be null or empty.");
            }
        }
    }
}
