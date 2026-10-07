using System.Linq.Expressions;
using VehicleServiceManagement.API.DTOs.Common;
using VehicleServiceManagement.API.Enums;
using VehicleServiceManagement.API.Models;

namespace VehicleServiceManagement.API.Repositories.Interfaces
{
    public interface IRepository<T> where T : BaseEntity
    {
        Task<PagedResultDto<T>> GetAllAsync(
            ArchiveStatus archiveStatus = ArchiveStatus.Active,
            SortOptionsDto? sortOptions = null,
            PaginationDto? pagination = null,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null);

        Task<T?> GetByIdAsync(int id, Func<IQueryable<T>, IQueryable<T>>? include = null);
        Task<T?> GetByIdIncludingArchivedAsync(int id);
        
        Task AddAsync(T entity);
        Task UpdateAsync(T entity, byte[]? originalRowVersion = null);
        Task ArchiveAsync(T entity, byte[] originalRowVersion);
        Task UnarchiveAsync(T entity, byte[] originalRowVersion);

        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T,bool>> predicate);
    }
}
