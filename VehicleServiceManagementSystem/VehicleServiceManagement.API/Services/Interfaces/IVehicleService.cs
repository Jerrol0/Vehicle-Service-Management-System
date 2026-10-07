using VehicleServiceManagement.API.DTOs.Common;
using VehicleServiceManagement.API.DTOs.Vehicles;
using VehicleServiceManagement.API.Enums;

namespace VehicleServiceManagement.API.Services.Interfaces
{
    public interface IVehicleService
    {
        Task<PagedResultDto<VehicleDto>> GetAllVehiclesAsync(
            string? searchTerm = null,
            ArchiveStatus archiveStatus = ArchiveStatus.Active,
            SortOptionsDto? sortOptions = null,
            PaginationDto? pagination = null);
        Task<VehicleDto?> GetVehicleByIdAsync(int id);
        Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto createVehicleDto);
        Task<VehicleDto> UpdateVehicleAsync(int id, UpdateVehicleDto updateVehicleDto);
        Task ArchiveVehicleAsync(int id, byte[] rowVersion);
        Task UnarchiveVehicleAsync(int id, byte[] rowVersion);
        Task<VehicleDto> ReassignVehicleAsync(int vehicleId, ReassignVehicleDto reassignVehicleDto);
    }
}
