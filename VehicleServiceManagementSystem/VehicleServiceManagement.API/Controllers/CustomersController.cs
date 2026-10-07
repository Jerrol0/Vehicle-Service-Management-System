using Microsoft.AspNetCore.Mvc;
using VehicleServiceManagement.API.DTOs.Common;
using VehicleServiceManagement.API.DTOs.Customers;
using VehicleServiceManagement.API.Enums;
using VehicleServiceManagement.API.Services.Interfaces;

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
        public async Task<ActionResult<PagedResultDto<CustomerDto>>> GetAllCustomers(
            [FromQuery] string? searchTerm = null,
            [FromQuery] ArchiveStatus archiveStatus = ArchiveStatus.Active,
            [FromQuery] SortOptionsDto? sortOptions = null,
            [FromQuery] PaginationDto? pagination = null)
        {
            var customers = await _customerService.GetAllCustomersAsync(
                searchTerm,
                archiveStatus,
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
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateCustomer(int id, UpdateCustomerDto updateCustomerDto)
        {
            await _customerService.UpdateCustomerAsync(id, updateCustomerDto);
            return NoContent();
        }

        [HttpPatch("{id}/archive")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ArchiveCustomer(
            int id, [FromBody] ArchiveRequestDto request)
        {       
            await _customerService.ArchiveCustomerAsync(id, request.RowVersion);
            return NoContent();
        }

        [HttpPatch("{id}/unarchive")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UnarchiveCustomer(
            int id, [FromBody] ArchiveRequestDto request)
        {
            await _customerService.UnarchiveCustomerAsync(id, request.RowVersion);
            return NoContent();
        }
    }
}
