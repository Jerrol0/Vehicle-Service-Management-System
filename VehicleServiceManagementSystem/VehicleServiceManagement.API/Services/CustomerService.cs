using System.Linq.Expressions;
using VehicleServiceManagement.API.Common.Exceptions;
using VehicleServiceManagement.API.DTOs.Common;
using VehicleServiceManagement.API.DTOs.Customers;
using VehicleServiceManagement.API.Enums;
using VehicleServiceManagement.API.Models;
using VehicleServiceManagement.API.Repositories.Interfaces;
using VehicleServiceManagement.API.Services.Interfaces;

namespace VehicleServiceManagement.API.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CustomerService(ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
        {
            _customerRepository = customerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResultDto<CustomerDto>> GetAllCustomersAsync(
            string? searchTerm = null,
            ArchiveStatus archiveStatus = ArchiveStatus.Active,
            SortOptionsDto? sortOptions = null,
            PaginationDto? pagination = null)
        {
            Expression<Func<Customer, bool>>? filter = null;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalizedSearchTerm = searchTerm.Trim();

                filter = customer =>
                    customer.FullName.Contains(normalizedSearchTerm) ||
                    customer.Email.Contains(normalizedSearchTerm) ||
                    customer.ContactNumber.Contains(normalizedSearchTerm) ||
                    customer.Address.Contains(normalizedSearchTerm);
            }

            var result = await _customerRepository.GetAllAsync(
                archiveStatus,
                sortOptions,
                pagination,
                filter);

            return new PagedResultDto<CustomerDto>
            {
                Items = result.Items.Select(MapToDto),
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                TotalPages = result.TotalPages
            };
        }

        public async Task<CustomerDto?> GetCustomerByIdAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);

            if (customer == null)
            {
                return null;
            }

            return MapToDto(customer);
        }

        public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto createCustomerDto)
        {
            await ValidateUniqueEmailAsync(createCustomerDto.Email);

            var customer = new Customer
            {
                FullName = createCustomerDto.FullName,
                ContactNumber = createCustomerDto.ContactNumber,
                Email = createCustomerDto.Email,
                Address = createCustomerDto.Address,
            };

            await _customerRepository.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();
            return MapToDto(customer);
        }

        public async Task<CustomerDto> UpdateCustomerAsync(int id, UpdateCustomerDto updateCustomerDto)
        {
            var customer = await ValidateCustomerAsync(id);

            if (customer.Email != updateCustomerDto.Email)
            {
                await ValidateUniqueEmailAsync(updateCustomerDto.Email, id);
            }

            customer.FullName = updateCustomerDto.FullName;
            customer.ContactNumber = updateCustomerDto.ContactNumber;
            customer.Email = updateCustomerDto.Email;
            customer.Address = updateCustomerDto.Address;

            await _customerRepository.UpdateAsync(
                customer,
                updateCustomerDto.RowVersion);
            await _unitOfWork.SaveChangesAsync();
            return MapToDto(customer);
        }

        public async Task ArchiveCustomerAsync(int id, byte[] rowVersion)
        {
            var customer = await _customerRepository.GetByIdIncludingArchivedAsync(id);

            if (customer == null)
            {
                throw new NotFoundException($"Customer with ID {id} not found.");
            }

            if (customer.IsArchived) 
            {
                throw new ValidationException($"Customer with ID {id} is already archived.");
            }

            await _customerRepository.ArchiveAsync(customer,rowVersion);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UnarchiveCustomerAsync(int id, byte[] rowVersion)
        {
            var customer = await _customerRepository.GetByIdIncludingArchivedAsync(id);

            if (customer == null)
            {
                throw new NotFoundException($"Customer with ID {id} not found.");
            }

            if (!customer.IsArchived)
            {
                throw new ValidationException($"Customer with ID {id} is not archived.");
            }

            await _customerRepository.UnarchiveAsync(customer, rowVersion);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<Customer> ValidateCustomerAsync(int customerId)
        {
            var customer = await _customerRepository.GetByIdAsync(customerId);

            if (customer == null)
            {
                throw new NotFoundException($"Customer with ID {customerId} not found.");
            }

            return customer;
        }

        private async Task ValidateUniqueEmailAsync(string email, int? currentCustomerId = null)
        {
            var existingCustomer = await _customerRepository.GetByEmailAsync(email);

            if (existingCustomer != null && existingCustomer.Id != currentCustomerId)
            {
                throw new ValidationException("A customer with this email already exists.");
            }
        }

        private static CustomerDto MapToDto(Customer customer)
        {
            return new CustomerDto
            {
                Id = customer.Id,
                FullName = customer.FullName,
                ContactNumber = customer.ContactNumber,
                Email = customer.Email,
                Address = customer.Address,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt,
                IsArchived = customer.IsArchived,
                RowVersion = customer.RowVersion
            };
        }
    }
}
