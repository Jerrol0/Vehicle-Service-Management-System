using VehicleServiceManagement.API.DTOs.Common;
using VehicleServiceManagement.API.DTOs.Customers;

namespace VehicleServiceManagement.API.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDto>> GetAllCustomersAsync(
            bool includeArchived = false,
            SortOptionsDto? sortOptions = null,
            PaginationDto? pagination = null);
        Task<CustomerDto?> GetCustomerByIdAsync(int id);
        Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto createCustomerDto);
        Task<CustomerDto> UpdateCustomerAsync(int id, UpdateCustomerDto updateCustomerDto);
        Task ArchiveCustomerAsync(int id);
        Task<IEnumerable<CustomerDto>> SearchCustomersAsync(string searchTerm);
    }
}
