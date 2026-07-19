using VehicleServiceManagement.API.Models;

namespace VehicleServiceManagement.API.Repositories.Interfaces
{
    public interface IServiceRecordRepository : IRepository<ServiceRecord>
    {
        Task<IEnumerable<ServiceRecord>> GetVehicleServiceHistoryAsync(int vehicleId);
    }
}
