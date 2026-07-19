using Microsoft.AspNetCore.Mvc;
using VehicleServiceManagement.API.Services.Interfaces;
using VehicleServiceManagement.API.DTOs.Customers;
using VehicleServiceManagement.API.DTOs.Common;

namespace VehicleServiceManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetAllCustomers(
            [FromQuery] bool includeArchived = false,
            [FromQuery] SortOptionsDto? sortOptions = null,
            [FromQuery] PaginationDto? pagination = null)
        {
            var customers = await _customerService.GetAllCustomersAsync(
                includeArchived,
                sortOptions,
                pagination);
            return Ok(customers);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetCustomerById(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);

            if (customer == null)
            {
                return NotFound($"Customer with ID {id} not found.");
            }

            return Ok(customer);
        }

        [HttpPost]
        public async Task<ActionResult<CustomerDto>> CreateCustomer(CreateCustomerDto createCustomerDto)
        {
            var createdCustomer = await _customerService.CreateCustomerAsync(createCustomerDto);

            return CreatedAtAction(
                nameof(GetCustomerById),
                new { id = createdCustomer.Id },
                createdCustomer);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, UpdateCustomerDto updateCustomerDto)
        {
            await _customerService.UpdateCustomerAsync(id, updateCustomerDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> ArchiveCustomer(int id)
        {       
            await _customerService.ArchiveCustomerAsync(id);
            return NoContent();
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> SearchCustomers([FromQuery] string searchTerm)
        {
            var customers = await _customerService.SearchCustomersAsync(searchTerm);
            return Ok(customers);
        }
    }
}
