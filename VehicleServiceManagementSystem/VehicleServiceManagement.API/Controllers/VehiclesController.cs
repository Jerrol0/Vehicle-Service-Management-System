using Microsoft.AspNetCore.Mvc;
using VehicleServiceManagement.API.DTOs.Common;
using VehicleServiceManagement.API.DTOs.Vehicles;
using VehicleServiceManagement.API.Enums;
using VehicleServiceManagement.API.Services;    
using VehicleServiceManagement.API.Services.Interfaces;

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
        public async Task<ActionResult<PagedResultDto<VehicleDto>>> GetAllVehicles(
            [FromQuery] string? searchTerm = null,
            [FromQuery] ArchiveStatus archiveStatus = ArchiveStatus.Active,
            [FromQuery] SortOptionsDto? sortOptions = null,
            [FromQuery] PaginationDto? pagination = null)
        {
            var vehicles = await _vehicleService.GetAllVehiclesAsync(
                searchTerm,
                archiveStatus,
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
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpdateVehicle(int id, UpdateVehicleDto updateVehicleDto)
        {
            await _vehicleService.UpdateVehicleAsync(id, updateVehicleDto);
            return NoContent();
        }

        [HttpPatch("{id}/archive")] 
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ArchiveVehicle(
            int id, [FromBody] ArchiveRequestDto request)
        {
            await _vehicleService.ArchiveVehicleAsync(id, request.RowVersion);
            return NoContent();
        }

        [HttpPatch("{id}/unarchive")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UnarchiveVehicle(
            int id, [FromBody] ArchiveRequestDto request)
        {
            await _vehicleService.UnarchiveVehicleAsync(id, request.RowVersion);
            return NoContent();
        }

        [HttpPut("{id}/reassign")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> ReassignVehicle(int id, ReassignVehicleDto reassignVehicleDto)
        {
            await _vehicleService.ReassignVehicleAsync(id, reassignVehicleDto);
            return NoContent();
        }
    }
}
