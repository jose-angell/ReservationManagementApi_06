using Microsoft.AspNetCore.Mvc;
using ReservationManagementApi_06.Application;
using ReservationManagementApi_06.Dtos.Customer;

namespace ReservationManagementApi_06.Controllers
{
    [ApiController]
    [Route("api/customers")]
    public class CustomerController : ControllerBase
    {
        private readonly CustomerUseCase _useCase;
        public CustomerController(CustomerUseCase useCase)
        {
            _useCase = useCase;
        }
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            if (id == Guid.Empty) return BadRequest("El id es invalido.");
            var customer = await _useCase.GetById(id);
            return Ok(customer);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomer request)
        {
            var newCustomer = await _useCase.Create(request);
            return CreatedAtAction(nameof(GetById), new { id = newCustomer.Id }, newCustomer);
        }
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateCustomer request)
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
        public async Task<IActionResult> GetAll()
        {
            var customers = await _useCase.GetAll();
            return Ok(customers);
        }
    }
}
