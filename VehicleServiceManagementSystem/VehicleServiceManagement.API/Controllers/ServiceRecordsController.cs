using Microsoft.AspNetCore.Mvc;
using VehicleServiceManagement.API.DTOs.Common;
using VehicleServiceManagement.API.DTOs.ServiceRecords;
using VehicleServiceManagement.API.Enums;
using VehicleServiceManagement.API.Services;
using VehicleServiceManagement.API.Services.Interfaces;

namespace VehicleServiceManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceRecordsController : ControllerBase
    {
        private readonly IServiceRecordService _serviceRecordService;

        public ServiceRecordsController(IServiceRecordService serviceRecordService)
        {
            _serviceRecordService = serviceRecordService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResultDto<ServiceRecordDto>>> GetAllServiceRecords(
            [FromQuery] string? searchTerm = null,
            [FromQuery] int? vehicleId = null,
            [FromQuery] ServiceType? serviceType = null,
            [FromQuery] ServiceStatus? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] ArchiveStatus archiveStatus = ArchiveStatus.Active,
            [FromQuery] SortOptionsDto? sortOptions = null,
            [FromQuery] PaginationDto? pagination = null)
        {
            var serviceRecords = await _serviceRecordService.GetAllServiceRecordsAsync(
                searchTerm,
                vehicleId,
                serviceType,
                status,
                startDate,
                endDate,
                archiveStatus,
                sortOptions,
                pagination);

            return Ok(serviceRecords);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ServiceRecordDto>> GetServiceRecordById(int id)
        {
            var serviceRecord = await _serviceRecordService.GetServiceRecordByIdAsync(id);

            if (serviceRecord == null)
            {
                return NotFound($"Service record with ID {id} not found.");
            }

            return Ok(serviceRecord);
        }

        [HttpPost]
        public async Task<ActionResult<ServiceRecordDto>> CreateServiceRecord(CreateServiceRecordDto createServiceRecordDto)
        {
            var createdServiceRecord = await _serviceRecordService.CreateServiceRecordAsync(createServiceRecordDto);

            return CreatedAtAction(
                nameof(GetServiceRecordById),
                new { id = createdServiceRecord.Id},
                createdServiceRecord);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateServiceRecord(int id, UpdateServiceRecordDto updateServiceRecordDto)
        {
            await _serviceRecordService.UpdateServiceRecordAsync(id, updateServiceRecordDto);
            return NoContent();
        }

        [HttpPatch("{id}/archive")]
        public async Task<IActionResult> ArchiveServiceRecord(
            int id, [FromBody] ArchiveRequestDto request)
        {
            await _serviceRecordService.ArchiveServiceRecordAsync(id, request.RowVersion);
            return NoContent();
        }

        [HttpPatch("{id}/unarchive")]
        public async Task<IActionResult> UnarchiveCustomer(
            int id, [FromBody] ArchiveRequestDto request)
        {
            await _serviceRecordService.UnarchiveServiceRecordAsync(id, request.RowVersion);
            return NoContent();
        }

        [HttpGet("/api/vehicles/{id}/services")]
        public async Task<ActionResult<IEnumerable<ServiceRecordDto>>> GetVehicleServiceHistory(int id)
        {
            var serviceRecords = await _serviceRecordService.GetVehicleServiceHistoryAsync(id);
            return Ok(serviceRecords);
        }
    }
}
