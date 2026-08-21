using Microsoft.EntityFrameworkCore;
using ReservationManagementApi_06.Domain;
using ReservationManagementApi_06.Dtos.Reservation;
using ReservationManagementApi_06.Exceptions;
using ReservationManagementApi_06.Infrastructure;
/* Estrategia para manejar concurrencia
 * la forma mas segura seria utilizado una exclusion constrains en la base de datos
 * confiugrando una funcion para validar el rango de tiempo (tsrange) con una restrincion de exclusion
 * impidiendo que alguien guarde un registro donde el ResourceId y el reango de fechas ya este registrado
 * 
 */
namespace ReservationManagementApi_06.Application
{
    public class ReservationUseCase
    {
        private readonly AppDbContext _context;
        public ReservationUseCase(AppDbContext context)
        {
            _context = context;
        }
        public async Task<ReservationDto> Create(CreateReservation request)
        {
            var customerId = request.CustomerId!.Value;

            var existCustomer = await _context.Customers.AnyAsync(c => c.Id == customerId);
            if (!existCustomer) throw new NotFoundException("El cliente no se encontro en el sistema.");

            var resource = await _context.Resources.FirstOrDefaultAsync(r => r.Id == request.ResourceId && r.IsActive == true);
            if (resource == null) throw new NotFoundException("El recurso no se encontro en el sistema.");

            if (request.StartDateTime >= request.EndDateTime) throw new ConflictException("las fechas no son validas.");

            var resourceId = request.ResourceId!.Value;
            var startUtc = request.StartDateTime!.Value.ToUniversalTime();
            var endUtc = request.EndDateTime!.Value.ToUniversalTime();

            var existConflict = await _context.Reservations
                .AnyAsync(r => r.ResourceId == resourceId && (r.Status == StatusReservation.Pending || r.Status == StatusReservation.Confirmed)
                && (startUtc < r.EndDateTime && endUtc > r.StartDateTime));
            if (existConflict) throw new ConflictException("Existe un conflicto entre los horarios seleccionados.");

            var totalPrice = CalculateTotalPrice(request.StartDateTime!.Value, request.EndDateTime!.Value, resource.HourlyRate);

            var newReservation = new Reservation(request.CustomerId!.Value, request.ResourceId!.Value, request.StartDateTime!.Value, request.EndDateTime!.Value, totalPrice);

            await _context.AddAsync(newReservation);

            await _context.SaveChangesAsync();

            return new ReservationDto
            {
                Id = newReservation.Id,
                CustomerId = newReservation.CustomerId,
                ResourceId = newReservation.ResourceId,
                StartDateTime = newReservation.StartDateTime,
                EndDateTime = newReservation.EndDateTime,
                TotalPrice = totalPrice,
                Status = newReservation.Status,
                CreatedAt = newReservation.CreatedAt,
            };
        }
        public async Task Update(Guid id, UpdateReservation request)
        {
            var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id);
            if (reservation == null) throw new NotFoundException("La reserva no se encontro en el sistema.");

            if(reservation.Status != StatusReservation.Pending ) throw new ConflictException("Solo las reservas penditentes pueden ser editadas");

            var resource = await _context.Resources.FirstOrDefaultAsync(r => r.Id == request.ResourceId && r.IsActive == true);
            if (resource == null) throw new NotFoundException("El recurso no se encontro en el sistema.");

            if (request.StartDateTime >= request.EndDateTime) throw new ConflictException("las fechas no son validas.");

            var existConflict = await _context.Reservations
                .AnyAsync(r => r.ResourceId == request.ResourceId && r.Id != id && (r.Status == StatusReservation.Pending || r.Status == StatusReservation.Confirmed)
                && (request.StartDateTime!.Value.ToUniversalTime() < r.EndDateTime && request.EndDateTime!.Value.ToUniversalTime() > r.StartDateTime));
            if (existConflict) throw new ConflictException("Existe un conflicto entre los horarios seleccionados.");

            var totalPrice = CalculateTotalPrice(request.StartDateTime!.Value, request.EndDateTime!.Value, resource.HourlyRate);

            reservation.Update(request.ResourceId!.Value, request.StartDateTime!.Value, request.EndDateTime!.Value, totalPrice);
            await _context.SaveChangesAsync();
        }
        public async Task Delete(Guid id)
        {
            var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id);
            if (reservation == null) throw new NotFoundException("La reserva no se encontro en el sistema.");

            if (reservation.Status != StatusReservation.Pending) throw new ConflictException("Solo las reservas pendientes pueden ser eliminadas.");

            _context.Remove(reservation);
            await _context.SaveChangesAsync();
        }
        public async Task<ReservationDto> GetById(Guid id)
        {
            var reservation = await _context.Reservations.AsNoTracking().Include(c => c.Customer)
                .Include(re => re.Resource).FirstOrDefaultAsync(r => r.Id == id);
            if (reservation == null) throw new NotFoundException("La reserva no se encontro en el sistema.");
            return MapToDto(reservation);
        }
        public async Task<IEnumerable<ReservationDto>> GetAll(ReservationQuery paramQuery)
        {
            IQueryable<Reservation> query = _context.Reservations.AsNoTracking();

            if (paramQuery.CustomerId.HasValue)
            {
                query = query.Where(r => r.CustomerId == paramQuery.CustomerId.Value);
            }
            if (paramQuery.ResourceId.HasValue)
            {
                query = query.Where(r => r.ResourceId == paramQuery.ResourceId.Value);
            }
            if (paramQuery.Status.HasValue)
            {
                query = query.Where(r => r.Status == paramQuery.Status.Value);
            }
            if (paramQuery.FromDate.HasValue)
            {
                query = query.Where(r => r.EndDateTime > paramQuery.FromDate!.Value.ToUniversalTime());
            }
            if (paramQuery.ToDate.HasValue)
            {
                query = query.Where(r => r.StartDateTime < paramQuery.ToDate!.Value.ToUniversalTime());
            }

            query = paramQuery.SortBy?.ToLower() switch
            {
                "customer_desc" => query.OrderByDescending(q => q.CustomerId),
                "customer_asc" => query.OrderBy(q => q.CustomerId),
                "status_desc" => query.OrderByDescending(q => q.Status),
                "status_asc" => query.OrderBy(q => q.Status),
                "resource_desc" => query.OrderByDescending(q => q.ResourceId),
                _ => query.OrderBy(q => q.ResourceId),
            };

            int pageSize = paramQuery.PageSize;
            int page = paramQuery.Page;
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            return await query.Select(reservation => new ReservationDto
            {
                Id = reservation.Id,
                CustomerId = reservation.CustomerId,
                CustomerName = reservation.Customer.FullName,
                ResourceId = reservation.ResourceId,
                ResourceName = reservation.Resource.Name,
                StartDateTime = reservation.StartDateTime,
                EndDateTime = reservation.EndDateTime,
                TotalPrice = reservation.TotalPrice,
                Status = reservation.Status,
                CreatedAt = reservation.CreatedAt,

            }).Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        public async Task Confirm(Guid id)
        {
            var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id);
            if (reservation == null) throw new NotFoundException("La reserva no se encontro en el sistema.");

            reservation.Confirm();

            await _context.SaveChangesAsync();
        }
        public async Task Cancel(Guid id)
        {
            var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id);
            if (reservation == null) throw new NotFoundException("La reserva no se encontro en el sistema.");

            reservation.Cancel();

            await _context.SaveChangesAsync();
        }
        public async Task Complete(Guid id)
        {
            var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id);
            if (reservation == null) throw new NotFoundException("La reserva no se encontro en el sistema.");

            reservation.Complete();

            await _context.SaveChangesAsync();
        }
        private decimal CalculateTotalPrice(DateTime startTime, DateTime endTime, decimal hourlyRate)
        {
            // 1. Restamos las fechas. Esto devuelve un TimeSpan.
            TimeSpan duration = endTime - startTime;

            // 2. Obtenemos el total de horas (es de tipo double, así que lo pasamos a decimal)

            decimal totalHours = (decimal)Math.Ceiling(duration.TotalHours);

            // 3. Multiplicamos por la tarifa
            decimal totalPrice = totalHours * hourlyRate;

            return totalPrice;
        }
        private static ReservationDto MapToDto(Reservation reservation) => new ReservationDto
        {
            Id = reservation.Id,
            CustomerId = reservation.CustomerId,
            CustomerName = reservation.Customer.FullName,
            ResourceId = reservation.ResourceId,
            ResourceName = reservation.Resource.Name,
            StartDateTime = reservation.StartDateTime,
            EndDateTime = reservation.EndDateTime,
            TotalPrice = reservation.TotalPrice,
            Status = reservation.Status,
            CreatedAt = reservation.CreatedAt,

        };
    }
}
