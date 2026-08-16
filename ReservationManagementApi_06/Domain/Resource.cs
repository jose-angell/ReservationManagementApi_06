using ReservationManagementApi_06.Exceptions;

namespace ReservationManagementApi_06.Domain
{
    public class Resource
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int Capacity { get; private set; }
        public decimal HourlyRate { get; private set; }
        public bool IsActive { get; private set; }

        public ICollection<Reservation> Reservations { get; } = new List<Reservation>();
        private Resource()
        {
            Name = string.Empty;
            Description = string.Empty;
        }

        public Resource(string name, string description, int capacity, decimal hourlyRate)
        {
            Validate(name, description, capacity, hourlyRate);
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            Capacity = capacity;
            HourlyRate = hourlyRate;
            IsActive = true;
        }
        public void Update(string name, string description, int capacity, decimal hourlyRate)
        {
            Validate(name, description, capacity, hourlyRate);
            Name = name;
            Description = description;
            Capacity = capacity;
            HourlyRate = hourlyRate;
        }
        public void Activate()
        {
            IsActive = true;
        }
        public void Deactivate()
        {
            IsActive = false;
        }
        public void UpdateHourlyRate(decimal newHourlyRate)
        {
            if (newHourlyRate < 0)
            {
                throw new DomainException("Hourly rate cannot be negative.");
            }
            HourlyRate = newHourlyRate;
        }
        public void UpdateCapacity(int newCapacity)
        {
            if (newCapacity <= 0)
            {
                throw new DomainException("Capacity must be greater than zero.");
            }
            Capacity = newCapacity;
        }
        private void Validate(string name, string description, int capacity, decimal hourlyRate)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new DomainException("Name cannot be null or empty.");
            }
            if (capacity <= 0)
            {
                throw new DomainException("Capacity must be greater than zero.");
            }
            if (hourlyRate < 0)
            {
                throw new DomainException("Hourly rate cannot be negative.");
            }
        }
    }
}
