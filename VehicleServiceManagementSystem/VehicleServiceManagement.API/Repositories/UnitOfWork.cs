using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Common.Exceptions;
using VehicleServiceManagement.API.Infrastructure;
using VehicleServiceManagement.API.Repositories.Interfaces;

namespace VehicleServiceManagement.API.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly VehicleServiceDbContext _dbContext;
        public UnitOfWork(VehicleServiceDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                throw new ConcurrencyException(
                    "The record was modified by another user. Please reload the data and try again.");
            }
        }
    }
}
