using VehicleServiceManagement.API.Models;

namespace VehicleServiceManagement.API.Repositories.Interfaces
{
    public interface IVehicleRepository : IRepository<Vehicle>
    {
        Task<Vehicle?> GetByPlateNumberAsync(string plateNumber);
    }
}
