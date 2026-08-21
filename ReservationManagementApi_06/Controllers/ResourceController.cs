using Microsoft.AspNetCore.Mvc;
using ReservationManagementApi_06.Application;
using ReservationManagementApi_06.Dtos.Resource;

namespace ReservationManagementApi_06.Controllers
{
    [ApiController]
    [Route("api/resources")]
    public class ResourceController : ControllerBase
    {
        private readonly ResourceUseCase _useCase;
        public ResourceController(ResourceUseCase useCase)
        {
            _useCase = useCase;
        }
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            if (id == Guid.Empty) return BadRequest("El id es invalido.");
            var resource = await _useCase.GetById(id);
            return Ok(resource);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateResource request)
        {
            var newResource = await _useCase.Create(request);
            return CreatedAtAction(nameof(GetById), new { id = newResource.Id }, newResource);
        }
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateResource request)
        {
            if (id == Guid.Empty) return BadRequest("El id es invalido.");
            await _useCase.Update(id, request);
            return NoContent();
        }
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            if (id == Guid.Empty) return BadRequest("El id es invalido.");
            await _useCase.Delete(id);
            return NoContent();
        }
        [HttpPatch("activate/{id:guid}")]
        public async Task<IActionResult> Activate([FromRoute] Guid id)
        {
            if (id == Guid.Empty) return BadRequest("El id es invalido.");
            await _useCase.Activate(id);
            return NoContent();
        }
        [HttpPatch("deactivate/{id:guid}")]
        public async Task<IActionResult> Dectivate([FromRoute] Guid id)
        {
            if (id == Guid.Empty) return BadRequest("El id es invalido.");
            await _useCase.Deactivate(id);
            return NoContent();
        }
        [HttpGet("search")]
        public async Task<IActionResult> GetAll([FromQuery] ResourceQuery query)
        {
            var result = await _useCase.GetAll(query);
            return Ok(result);
        }
        [HttpGet("{resourceId:guid}/availability")]
        public async Task<IActionResult> Availability([FromRoute] Guid resourceId, [FromQuery] DateTime startDateTime, [FromQuery] DateTime endDateTime)
        {
            if (resourceId == Guid.Empty) return BadRequest("El id es invalido.");
            var result = await _useCase.Availability(resourceId, startDateTime, endDateTime);
            return Ok(result);
        }
        [HttpGet("availability")]
        public async Task<IActionResult> AvailabilityRangeDates( [FromQuery] DateTime startDateTime, [FromQuery] DateTime endDateTime)
        {
            var result = await _useCase.GetAvailableResources(startDateTime, endDateTime);
            return Ok(result);
        }
    }
}
