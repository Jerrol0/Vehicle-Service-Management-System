using System.Linq.Expressions;
using VehicleServiceManagement.API.DTOs.Common;
using VehicleServiceManagement.API.Models;

namespace VehicleServiceManagement.API.Repositories.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        // Define CRUD methods
        Task<IEnumerable<T>> GetAllAsync(
            bool includeArchived = false,
            SortOptionsDto? sortOptions = null,
            PaginationDto? pagination = null);
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity, byte[]? originalRowVersion = null);
        Task ArchiveAsync(T entity);

        // Define search and filter methods
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T,bool>> predicate);
    }
}
