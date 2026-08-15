using Microsoft.AspNetCore.Mvc;
using ReservationManagementApi_06.Application;
using ReservationManagementApi_06.Dtos.Reservation;

namespace ReservationManagementApi_06.Controllers
{
    [ApiController]
    [Route("api/reservations")]
    public class ReservationController: ControllerBase
    {
        private readonly ReservationUseCase _useCase;
        public ReservationController(ReservationUseCase useCase)
        {
            _useCase = useCase;
        }
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            if (id == Guid.Empty) return BadRequest("El id es invalido.");
            var result = await _useCase.GetById(id);
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateReservation request)
        {
            var result = await _useCase.Create(request);
            return CreatedAtAction(nameof(GetById), new {id = result.Id}, result);
        }
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateReservation request)
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
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ReservationQuery query)
        {
            var result = await _useCase.GetAll(query);
            return Ok(result);
        }
    }
}
