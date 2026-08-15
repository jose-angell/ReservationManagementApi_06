using Microsoft.EntityFrameworkCore;
using ReservationManagementApi_06.Domain;
using ReservationManagementApi_06.Dtos.Reservation;
using ReservationManagementApi_06.Dtos.Resource;
using ReservationManagementApi_06.Exceptions;
using ReservationManagementApi_06.Infrastructure;

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
            var existCustomer = await _context.Customers.AnyAsync(c => c.Id == request.CustomerId);
            if (!existCustomer) throw new NotFoundException("El cliente no se encontro en el sistema.");

            var resource = await _context.Resources.FirstOrDefaultAsync(r => r.Id == request.ResourceId);
            if (resource == null) throw new NotFoundException("El recurso no se encontro en el sistema.");

            if (request.StartDateTime >= request.EndDateTime) throw new ConflictException("las fechas no son validas.");

            var existConflict = await _context.Reservations
                .AnyAsync(r => r.ResourceId == request.ResourceId
                && (request.StartDateTime < r.EndDateTime && request.EndDateTime > r.StartDateTime));
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

            var resource = await _context.Resources.FirstOrDefaultAsync(r => r.Id == request.ResourceId);
            if (resource == null) throw new NotFoundException("El recurso no se encontro en el sistema.");

            if (request.StartDateTime >= request.EndDateTime) throw new ConflictException("las fechas no son validas.");

            var existConflict = await _context.Reservations
                .AnyAsync(r => r.ResourceId == request.ResourceId && r.Id != id
                && (request.StartDateTime < r.EndDateTime && request.EndDateTime > r.StartDateTime));
            if (existConflict) throw new ConflictException("Existe un conflicto entre los horarios seleccionados.");

            var totalPrice = CalculateTotalPrice(request.StartDateTime!.Value, request.EndDateTime!.Value, resource.HourlyRate);

            reservation.Update(request.ResourceId!.Value, request.StartDateTime!.Value, request.EndDateTime!.Value, totalPrice);
            await _context.SaveChangesAsync();
        }
        public async Task Delete(Guid id)
        {
            var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id);
            if (reservation == null) throw new NotFoundException("La reserva no se encontro en el sistema.");

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
                query = query.Where(r => r.StartDateTime >= paramQuery.FromDate.Value);
            }
            if (paramQuery.ToDate.HasValue)
            {
                query = query.Where(r => r.EndDateTime <= paramQuery.ToDate.Value);
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

            return await query.Select(r => MapToDto(r))
                .Skip((page -1) * pageSize)
                .Take(page)
                .ToListAsync();
        }
        public async Task<AvailabilityDto> Availability(Guid resourceId, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            var existConflict = await _context.Reservations.AsNoTracking()
                .Select(r =>  MapToDto(r)).Where(r => r.ResourceId == resourceId
                && (startTime < r.EndDateTime && endTime > r.StartDateTime)).ToListAsync();

            if (existConflict.Any())
            {
                return new AvailabilityDto
                {
                    ResourceId = resourceId,
                    ResourceName = "",
                    StartDateTime = startTime,
                    EndDateTime = endTime,
                    IsAvailable = false,
                    conflictingReservations = existConflict
                };
            }
            return new AvailabilityDto
            {
                ResourceId = resourceId,
                ResourceName = "",
                StartDateTime = startTime,
                EndDateTime = endTime,
                IsAvailable = true,
                conflictingReservations = existConflict
            };
        }
        private decimal CalculateTotalPrice(DateTimeOffset startTime, DateTimeOffset endTime, decimal hourlyRate)
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

        };
    }
}
