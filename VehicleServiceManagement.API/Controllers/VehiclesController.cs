using Microsoft.AspNetCore.Mvc;
using VehicleServiceManagement.API.Services.Interfaces;
using VehicleServiceManagement.API.DTOs.Vehicles;
using VehicleServiceManagement.API.DTOs.Common;

namespace VehicleServiceManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VehiclesController : ControllerBase
    {
        private readonly IVehicleService _vehicleService;

        public VehiclesController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<VehicleDto>>> GetAllVehicles(
            [FromQuery] bool includeArchived = false,
            [FromQuery] SortOptionsDto? sortOptions = null,
            [FromQuery] PaginationDto? pagination = null)
        {
            var vehicles = await _vehicleService.GetAllVehiclesAsync(
                includeArchived,
                sortOptions,
                pagination);
            return Ok(vehicles);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleDto>> GetVehicleById(int id) 
        {
            var vehicle = await _vehicleService.GetVehicleByIdAsync(id);

            if (vehicle == null)
            {
                return NotFound($"Vehicle with ID {id} not found.");
            }

            return Ok(vehicle);
        }

        [HttpPost]
        public async Task<ActionResult<VehicleDto>> CreateVehicle(CreateVehicleDto createVehicleDto)
        {
            var createdVehicle = await _vehicleService.CreateVehicleAsync(createVehicleDto);

            return CreatedAtAction(
                nameof(GetVehicleById),
                new { id = createdVehicle.Id },
                createdVehicle); 
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, UpdateVehicleDto updateVehicleDto)
        {
            await _vehicleService.UpdateVehicleAsync(id, updateVehicleDto);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> ArchiveVehicle(int id)
        {
            await _vehicleService.ArchiveVehicleAsync(id);
            return NoContent();
        }

        [HttpPut("{id}/reassign")]
        public async Task<IActionResult> ReassignVehicle(int id, ReassignVehicleDto reassignVehicleDto)
        {
            await _vehicleService.ReassignVehicleAsync(id, reassignVehicleDto);
            return NoContent();
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<VehicleDto>>> SearchVehicles([FromQuery] string searchTerm)
        {
            var vehicles = await _vehicleService.SearchVehiclesAsync(searchTerm);
            return Ok(vehicles);
        }
    }
}
