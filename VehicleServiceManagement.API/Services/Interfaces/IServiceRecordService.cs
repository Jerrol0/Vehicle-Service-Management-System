using VehicleServiceManagement.API.DTOs.Common;
using VehicleServiceManagement.API.DTOs.ServiceRecords;

namespace VehicleServiceManagement.API.Services.Interfaces
{
    public interface IServiceRecordService
    {
        Task<IEnumerable<ServiceRecordDto>> GetAllServiceRecordsAsync(
            bool includeArchived = false,
            SortOptionsDto? sortOptions = null,
            PaginationDto? pagination = null);
        Task<ServiceRecordDto?> GetServiceRecordByIdAsync(int id);
        Task<ServiceRecordDto> CreateServiceRecordAsync(CreateServiceRecordDto createServiceRecordDto);
        Task<ServiceRecordDto> UpdateServiceRecordAsync(int id, UpdateServiceRecordDto updateServiceRecordDto);
        Task ArchiveServiceRecordAsync(int id);
        Task<IEnumerable<ServiceRecordDto>> GetVehicleServiceHistoryAsync(int vehicleId);
        Task<IEnumerable<ServiceRecordDto>> FilterServiceRecordsAsync(ServiceRecordFilterDto filterDto);
    }
}
