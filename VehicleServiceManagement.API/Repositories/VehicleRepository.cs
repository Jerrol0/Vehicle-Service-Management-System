using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Infrastructure;
using VehicleServiceManagement.API.Models;
using VehicleServiceManagement.API.Repositories.Interfaces;
using VehicleServiceManagement.API.Infrastructure.Sorting;

namespace VehicleServiceManagement.API.Repositories
{
    public class VehicleRepository : Repository<Vehicle>, IVehicleRepository
    {
        public VehicleRepository(
            VehicleServiceDbContext dbContext,
            ISortExpressionProvider<Vehicle> sortProvider)
            : base(dbContext, sortProvider)
        {
        }

        public async Task<Vehicle?> GetByPlateNumberAsync(string plateNumber)
        {
            return await _dbSet.FirstOrDefaultAsync(v => v.PlateNumber == plateNumber && !v.IsArchived);
        }
    }
}
