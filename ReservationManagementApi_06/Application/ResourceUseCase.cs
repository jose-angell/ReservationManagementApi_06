using Microsoft.EntityFrameworkCore;
using ReservationManagementApi_06.Domain;
using ReservationManagementApi_06.Dtos.Reservation;
using ReservationManagementApi_06.Dtos.Resource;
using ReservationManagementApi_06.Exceptions;
using ReservationManagementApi_06.Infrastructure;

namespace ReservationManagementApi_06.Application
{
    public class ResourceUseCase
    {
        private readonly AppDbContext _context;
        public ResourceUseCase(AppDbContext context)
        {
            _context = context;
        }
        public async Task<ResourceDto> Create(CreateResource request)
        {
            var existResourceName = await _context.Resources.AnyAsync(r => r.Name == request.Name);
            if (existResourceName) throw new ConflictException("Ya existe un recurso con el mismmo nombre.");

            var newResource = new Resource(request.Name!, request.Description!, request.Capacity, request.HourlyRate);

            await _context.Resources.AddAsync(newResource);
            await _context.SaveChangesAsync();

            return new ResourceDto
            {
                Id = newResource.Id,
                Name = newResource.Name,
                Description = newResource.Description,
                Capacity = newResource.Capacity,
                HourlyRate = newResource.HourlyRate,
                IsActive = newResource.IsActive
            };
        }
        public async Task Update(Guid id, UpdateResource request)
        {
            var resource = await _context.Resources.FirstOrDefaultAsync(r => r.Id == id);
            if (resource == null) throw new NotFoundException("No se en contro el recurso en el sistema.");

            var nameExist = await _context.Resources.AnyAsync(r => r.Id != id && r.Name == request.Name);
            if (nameExist) throw new ConflictException("Ya existe un recurso con el mismmo nombre.");

            resource.Update(request.Name!, request.Description!, request.Capacity, request.HourlyRate);

            await _context.SaveChangesAsync();
        }
        public async Task Delete(Guid id)
        {
            var resource = await _context.Resources.FirstOrDefaultAsync(r => r.Id == id);
            if (resource == null) throw new NotFoundException("No se en contro el recurso en el sistema.");

            var hasReservation = await _context.Reservations.AnyAsync(r => r.ResourceId == id);
            if (hasReservation) throw new ConflictException("No se puede eliminar un recurso con reservaciones registradas.");

            _context.Resources.Remove(resource);

            await _context.SaveChangesAsync();
        }
        public async Task Activate(Guid id)
        {
            var resource = await _context.Resources.FirstOrDefaultAsync(r => r.Id == id);
            if (resource == null) throw new NotFoundException("No se en contro el recurso en el sistema.");

            resource.Activate();

            await _context.SaveChangesAsync();
        }
        public async Task Deactivate(Guid id)
        {
            var resource = await _context.Resources.FirstOrDefaultAsync(r => r.Id == id);
            if (resource == null) throw new NotFoundException("No se en contro el recurso en el sistema.");

            resource.Deactivate();

            await _context.SaveChangesAsync();
        }
        public async Task<ResourceDto> GetById(Guid id)
        {
            var resource = await _context.Resources.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);

            if (resource == null) throw new NotFoundException("No se en contro el recurso en el sistema.");

            return MapToDto(resource);
        }
        public async Task<IEnumerable<ResourceDto>> GetAll(ResourceQuery paramQuery)
        {
            IQueryable<Resource> query = _context.Resources.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(paramQuery.Name))
            {
                query = query.Where(r => r.Name.ToLower().Contains(paramQuery.Name.ToLower()));
            }
            if (!string.IsNullOrWhiteSpace(paramQuery.Description))
            {
                query = query.Where(r => r.Description.ToLower().Contains(paramQuery.Description.ToLower()));
            }
            if (paramQuery.MinCapacity.HasValue)
            {
                query = query.Where(r => r.Capacity >= paramQuery.MinCapacity.Value);
            }
            if (paramQuery.MaxCapacity.HasValue)
            {
                query = query.Where(r => r.Capacity <= paramQuery.MaxCapacity.Value);
            }
            if (paramQuery.MinHourlyRate.HasValue)
            {
                query = query.Where(r => r.HourlyRate >= paramQuery.MinHourlyRate.Value);
            }
            if (paramQuery.MaxHourlyRate.HasValue)
            {
                query = query.Where(r => r.HourlyRate <= paramQuery.MaxHourlyRate.Value);
            }
            int pageSize = paramQuery.PageSize;
            int page = paramQuery.PageNumber;
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 100) pageSize = 10;

            return await query
                .Select(resource => new ResourceDto
                {
                    Id = resource.Id,
                    Name = resource.Name,
                    Description = resource.Description,
                    Capacity = resource.Capacity,
                    HourlyRate = resource.HourlyRate,
                    IsActive = resource.IsActive,
                })
                .OrderBy(r => r.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
        public async Task<AvailabilityDto> Availability(Guid resourceId, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            var existResource = await _context.Resources.AnyAsync(r => r.Id == resourceId);
            if (!existResource) throw new NotFoundException("El recurso no se encontro en el sistema.");

            if (startTime >= endTime) throw new ConflictException("las fechas no son validas.");

            var existConflict = await _context.Reservations.AsNoTracking()
                .Where(r => r.ResourceId == resourceId 
                && (r.Status == StatusReservation.Pending || r.Status == StatusReservation.Confirmed)
                && (startTime.ToUniversalTime() < r.EndDateTime && endTime.ToUniversalTime() > r.StartDateTime))
                .Select(reservation => new ReservationDto
                {
                    Id = reservation.Id,
                    CustomerId = reservation.CustomerId,
                    CustomerName = reservation.Customer.FullName,
                    ResourceId = reservation.ResourceId,
                    ResourceName = reservation.Resource.Name,
                    StartDateTime = reservation.StartDateTime,
                    EndDateTime = reservation.EndDateTime,
                    TotalPrice = reservation.TotalPrice,

                })
                .ToListAsync();

            var resource = await _context.Resources.AsNoTracking().FirstOrDefaultAsync(r => r.Id == resourceId);

            if (existConflict.Any())
            {
                return new AvailabilityDto
                {
                    ResourceId = resourceId,
                    ResourceName = resource!.Name,
                    StartDateTime = startTime,
                    EndDateTime = endTime,
                    IsAvailable = false,
                    conflictingReservations = existConflict
                };
            }
            return new AvailabilityDto
            {
                ResourceId = resourceId,
                ResourceName = resource!.Name,
                StartDateTime = startTime,
                EndDateTime = endTime,
                IsAvailable = true,
                conflictingReservations = existConflict
            };
        }
        private static ResourceDto MapToDto(Resource resource) => new ResourceDto
        {
            Id = resource.Id,
            Name = resource.Name,
            Description = resource.Description,
            Capacity = resource.Capacity,
            HourlyRate = resource.HourlyRate,
            IsActive = resource.IsActive,
        };
    }
}
