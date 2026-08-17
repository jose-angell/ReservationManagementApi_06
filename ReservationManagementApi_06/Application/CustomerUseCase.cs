using Microsoft.EntityFrameworkCore;
using ReservationManagementApi_06.Domain;
using ReservationManagementApi_06.Dtos.Customer;
using ReservationManagementApi_06.Exceptions;
using ReservationManagementApi_06.Infrastructure;

namespace ReservationManagementApi_06.Application
{
    public class CustomerUseCase
    {
        private readonly AppDbContext _context;
        public CustomerUseCase(AppDbContext context)
        {
            _context = context;
        }
        public async Task<CustomerDto> Create(CreateCustomer request)
        {
            var existEmail = await _context.Customers.AnyAsync(c => c.Email == request.Email);
            if (existEmail)
            {
                throw new ConflictException("El correo ya se encutra en uso.");
            }
            var newCustomer = new Customer(request.FullName!, request.Email!);
            await _context.AddAsync(newCustomer);
            await _context.SaveChangesAsync();
            return MapToDto(newCustomer);

        }
        public async Task Update(Guid id, UpdateCustomer request)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                throw new NotFoundException("Cliente no encontrado.");
            }
            var existEmail = await _context.Customers.AnyAsync(c => c.Email == request.Email && c.Id != id);
            if (existEmail)
            {
                throw new ConflictException("El correo ya se encutra en uso.");
            }
            customer.Update(request.FullName!, request.Email!);
            await _context.SaveChangesAsync();
        }
        public async Task Delete(Guid id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                throw new NotFoundException("Cliente no encontrado.");
            }
            var hasResevation = await _context.Reservations.AnyAsync(c => c.ResourceId == id);
            if (hasResevation) throw new ConflictException("No se puede eliminar un cliente con reservaciones registradas.");

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
        }
        public async Task<CustomerDto> GetById(Guid id)
        {
            var customer = await _context.Customers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (customer == null)
            {
                throw new NotFoundException($"Customer with id {id}");
            }
            return MapToDto(customer);
        }
        public async Task<IEnumerable<CustomerDto>> GetAll()
        {
            var customers = await _context.Customers.AsNoTracking().ToListAsync();
            return customers.Select(MapToDto);
        }
        private static CustomerDto MapToDto(Customer customer) => new CustomerDto
        {
            Id = customer.Id,
            FullName = customer.FullName,
            Email = customer.Email,
            CreatedAt = customer.CreatedAt,
        };
    }
}
