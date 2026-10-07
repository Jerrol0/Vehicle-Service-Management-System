using VehicleServiceManagement.API.DTOs.Common;
using VehicleServiceManagement.API.DTOs.Customers;
using VehicleServiceManagement.API.Enums;

namespace VehicleServiceManagement.API.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<PagedResultDto<CustomerDto>> GetAllCustomersAsync(
            string? searchTerm = null,
            ArchiveStatus archiveStatus = ArchiveStatus.Active,
            SortOptionsDto? sortOptions = null,
            PaginationDto? pagination = null);
        Task<CustomerDto?> GetCustomerByIdAsync(int id);
        Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto createCustomerDto);
        Task<CustomerDto> UpdateCustomerAsync(int id, UpdateCustomerDto updateCustomerDto);
        Task ArchiveCustomerAsync(int id, byte[] rowVersion);
        Task UnarchiveCustomerAsync(int id, byte[] rowVersion);
    }
}
