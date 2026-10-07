using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Infrastructure;
using VehicleServiceManagement.API.Infrastructure.Sorting;
using VehicleServiceManagement.API.Models;
using VehicleServiceManagement.API.Repositories.Interfaces;

namespace VehicleServiceManagement.API.Repositories
{
    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        public CustomerRepository(
            VehicleServiceDbContext dbContext,
            ISortExpressionProvider<Customer> sortProvider) 
            : base(dbContext, sortProvider)
        {
        }
        public async Task<Customer?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.Email == email && !c.IsArchived);
        }
    }
}
