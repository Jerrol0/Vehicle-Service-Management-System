using VehicleServiceManagement.API.DTOs.Common;
using VehicleServiceManagement.API.DTOs.Vehicles;

namespace VehicleServiceManagement.API.Services.Interfaces
{
    public interface IVehicleService
    {
        Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync(
            bool includeArchived = false,
            SortOptionsDto? sortOptions = null,
            PaginationDto? pagination = null);
        Task<VehicleDto?> GetVehicleByIdAsync(int id);
        Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto createVehicleDto);
        Task<VehicleDto> UpdateVehicleAsync(int id, UpdateVehicleDto updateVehicleDto);
        Task ArchiveVehicleAsync(int id);
        Task<VehicleDto> ReassignVehicleAsync(int vehicleId, ReassignVehicleDto reassignVehicleDto);
        Task<IEnumerable<VehicleDto>> SearchVehiclesAsync(string searchTerm);
    }
}
