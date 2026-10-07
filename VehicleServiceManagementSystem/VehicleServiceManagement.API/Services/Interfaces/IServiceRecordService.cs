using VehicleServiceManagement.API.DTOs.Common;
using VehicleServiceManagement.API.DTOs.ServiceRecords;
using VehicleServiceManagement.API.Enums;

namespace VehicleServiceManagement.API.Services.Interfaces
{
    public interface IServiceRecordService
    {
        Task<PagedResultDto<ServiceRecordDto>> GetAllServiceRecordsAsync(
            string? searchTerm = null,
            int? vehicleId = null,
            ServiceType? serviceType = null,
            ServiceStatus? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            ArchiveStatus archiveStatus = ArchiveStatus.Active,
            SortOptionsDto? sortOptions = null,
            PaginationDto? pagination = null);
        Task<ServiceRecordDto?> GetServiceRecordByIdAsync(int id);
        Task<ServiceRecordDto> CreateServiceRecordAsync(CreateServiceRecordDto createServiceRecordDto);
        Task<ServiceRecordDto> UpdateServiceRecordAsync(int id, UpdateServiceRecordDto updateServiceRecordDto);
        Task ArchiveServiceRecordAsync(int id, byte[] rowVersion);
        Task UnarchiveServiceRecordAsync(int id, byte[] rowVersion);
        Task<IEnumerable<ServiceRecordDto>> GetVehicleServiceHistoryAsync(int vehicleId);
    }
}
