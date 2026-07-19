using Microsoft.AspNetCore.Mvc;
using VehicleServiceManagement.API.DTOs.Common;
using VehicleServiceManagement.API.DTOs.ServiceRecords;
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
        public async Task<ActionResult<IEnumerable<ServiceRecordDto>>> GetAllServiceRecords(
            [FromQuery] bool includeArchived = false,
            [FromQuery] SortOptionsDto? sortOptions = null,
            [FromQuery] PaginationDto? pagination = null)
        {
            var serviceRecords = await _serviceRecordService.GetAllServiceRecordsAsync(
                includeArchived,
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> ArchiveServiceRecord(int id)
        {
            await _serviceRecordService.ArchiveServiceRecordAsync(id);
            return NoContent();
        }

        [HttpGet("/api/vehicles/{id}/services")]
        public async Task<ActionResult<IEnumerable<ServiceRecordDto>>> GetVehicleServiceHistory(int id)
        {
            var serviceRecords = await _serviceRecordService.GetVehicleServiceHistoryAsync(id);
            return Ok(serviceRecords);
        }

        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<ServiceRecordDto>>> FilterServiceRecords(
            [FromQuery] ServiceRecordFilterDto filterDto)
        {
            var serviceRecords = await _serviceRecordService.FilterServiceRecordsAsync(filterDto);
            return Ok(serviceRecords);
        }
    }
}
