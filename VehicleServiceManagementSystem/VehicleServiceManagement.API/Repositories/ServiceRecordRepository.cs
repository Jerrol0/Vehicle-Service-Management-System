using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Infrastructure;
using VehicleServiceManagement.API.Models;
using VehicleServiceManagement.API.Repositories.Interfaces; 
using VehicleServiceManagement.API.Infrastructure.Sorting;

namespace VehicleServiceManagement.API.Repositories
{
    public class ServiceRecordRepository :  Repository<ServiceRecord>, IServiceRecordRepository
    {
        public ServiceRecordRepository(
            VehicleServiceDbContext dbContext,
            ISortExpressionProvider<ServiceRecord> sortProvider)
            : base(dbContext, sortProvider)
        {
        }

        public async Task<IEnumerable<ServiceRecord>> GetVehicleServiceHistoryAsync(int vehicleId)
        {
            return await _dbSet
                .Include(sr => sr.Vehicle)
                .ThenInclude(vehicle => vehicle.Customer)
                .Where(sr => sr.VehicleId == vehicleId && !sr.IsArchived)
                .OrderByDescending(sr => sr.ServiceDate)
                .ToListAsync();
        }

        /* Future Enhancement
         FilterServiceRecordAsync will be implemented here */
    }
}
