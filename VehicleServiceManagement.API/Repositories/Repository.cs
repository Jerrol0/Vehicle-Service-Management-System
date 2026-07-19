using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VehicleServiceManagement.API.DTOs.Common;
using VehicleServiceManagement.API.Infrastructure;
using VehicleServiceManagement.API.Models;
using VehicleServiceManagement.API.Repositories.Interfaces;
using VehicleServiceManagement.API.Infrastructure.Sorting;
using VehicleServiceManagement.API.Common.Exceptions;

namespace VehicleServiceManagement.API.Repositories
{
    public class Repository<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly VehicleServiceDbContext _dbContext;
        protected readonly DbSet<T> _dbSet;
        private readonly ISortExpressionProvider<T> _sortProvider;

        public Repository(
            VehicleServiceDbContext dbContext,
            ISortExpressionProvider<T> sortProvider)
        {
            _dbContext = dbContext;
            _dbSet = _dbContext.Set<T>();
            _sortProvider = sortProvider;
        }

        public async Task<IEnumerable<T>> GetAllAsync(
            bool includeArchived = false,
            SortOptionsDto? sortOptions = null,
            PaginationDto? pagination = null)
        {
            IQueryable<T> query = _dbSet
                .Where(e => includeArchived || !e.IsArchived);

            if (!string.IsNullOrWhiteSpace(sortOptions?.SortBy))
            {
                var sortKey = sortOptions.SortBy.Trim().ToLower();

                if (!_sortProvider.SortExpressions.TryGetValue(sortKey, out var sortExpression))
                {
                    throw new ValidationException(
                        $"Invalid sort field '{sortOptions.SortBy}'.");
                }

                query = sortOptions.Descending
                    ? query.OrderByDescending(sortExpression)
                    : query.OrderBy(sortExpression);
            }

            if (pagination != null)
            {
                query = query
                    .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                    .Take(pagination.PageSize);
            }

            return await query.ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsArchived); 
        }

        public async Task AddAsync(T entity)
        {
            entity.CreatedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            await _dbSet.AddAsync(entity);
        }

        public Task UpdateAsync(T entity, byte[]? originalRowVersion = null)
        {       
            entity.UpdatedAt = DateTime.UtcNow;

            if (originalRowVersion != null)
            {
                _dbContext.Entry(entity)
                    .Property(e => e.RowVersion)
                    .OriginalValue = originalRowVersion;

            }
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }   

        public Task ArchiveAsync(T entity)
        {
            entity.IsArchived = true;
            entity.UpdatedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }
                                
        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate)
                .Where(e => !e.IsArchived)
                .ToListAsync();
        }
    }
}
    