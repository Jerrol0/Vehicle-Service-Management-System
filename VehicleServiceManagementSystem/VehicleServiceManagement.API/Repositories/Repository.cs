using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VehicleServiceManagement.API.DTOs.Common;
using VehicleServiceManagement.API.Infrastructure;
using VehicleServiceManagement.API.Models;
using VehicleServiceManagement.API.Repositories.Interfaces;
using VehicleServiceManagement.API.Infrastructure.Sorting;
using VehicleServiceManagement.API.Common.Exceptions;
using VehicleServiceManagement.API.Enums;

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

        public async Task<PagedResultDto<T>> GetAllAsync(
            ArchiveStatus archiveStatus = ArchiveStatus.Active,
            SortOptionsDto? sortOptions = null,
            PaginationDto? pagination = null,
            Expression<Func<T, bool>>? filter = null,
            Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            IQueryable<T> query = _dbSet;

            if (include != null)
            {
                query = include(query);
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }
                
            query = archiveStatus switch
            {
                ArchiveStatus.Active => query.Where(e => !e.IsArchived),
                ArchiveStatus.Archived => query.Where(e => e.IsArchived),
                ArchiveStatus.All => query,
                _ => throw new ValidationException(
                    $"Invalid archive status '{archiveStatus}'.")
            };

            if (!string.IsNullOrWhiteSpace(sortOptions?.SortBy))
            {
                var sortKey = sortOptions.SortBy.Trim().ToLower();

                if (!_sortProvider.SortExpressions.TryGetValue(
                    sortKey, 
                    out var sortExpression))
                {
                    throw new ValidationException($"Invalid sort field '{sortOptions.SortBy}'.");
                }

                query = sortOptions.Descending
                    ? query.OrderByDescending(sortExpression)
                    : query.OrderBy(sortExpression);
            }

            pagination ??= new PaginationDto();

            var totalCount = await query.CountAsync();

            var totalPages = (int)Math.Ceiling(
                totalCount / (double)pagination.PageSize);

            var items = await query
                .Skip((pagination.PageNumber - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();

            return new PagedResultDto<T>
            {
                Items = items,
                PageNumber = pagination.PageNumber,
                PageSize = pagination.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages
            };
        }

        public async Task<T?> GetByIdAsync(int id, Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            IQueryable<T> query = _dbSet;

            if (include != null)
            {
                query = include(query);
            }

            return await query.FirstOrDefaultAsync(e => e.Id == id && !e.IsArchived);
        }

        public async Task<T?> GetByIdIncludingArchivedAsync(int id)
        {
            return await _dbSet.FirstOrDefaultAsync(e => e.Id == id);
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

        public Task ArchiveAsync(
            T entity, byte[] originalRowVersion)
        {
            entity.IsArchived = true;
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
                                
        public Task UnarchiveAsync(
            T entity, byte[] originalRowVersion)
        {
            entity.IsArchived = false;
            entity.UpdatedAt = DateTime.UtcNow;

            _dbContext.Entry(entity)
                .Property(e => e.RowVersion)
                .OriginalValue = originalRowVersion;


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
    