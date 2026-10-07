using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.DTOs.Customers;
using VehicleServiceManagement.API.Common.Exceptions;
using VehicleServiceManagement.API.DTOs.Common;
using VehicleServiceManagement.API.DTOs.Vehicles;
using VehicleServiceManagement.API.Enums;
using VehicleServiceManagement.API.Models;
using VehicleServiceManagement.API.Repositories.Interfaces;
using VehicleServiceManagement.API.Services.Interfaces;

namespace VehicleServiceManagement.API.Services
{
        public class VehicleService : IVehicleService
        {
            private readonly IVehicleRepository _vehicleRepository;
            private readonly ICustomerRepository _customerRepository;
            private readonly IUnitOfWork _unitOfWork;

            public VehicleService(IVehicleRepository vehicleRepository, ICustomerRepository customerRepository, IUnitOfWork unitOfWork)
            {
                _vehicleRepository = vehicleRepository;
                _customerRepository = customerRepository;
                _unitOfWork = unitOfWork;
            }

        public async Task<PagedResultDto<VehicleDto>> GetAllVehiclesAsync(
            string? searchTerm = null,
            ArchiveStatus archiveStatus = ArchiveStatus.Active,
            SortOptionsDto? sortOptions = null,
            PaginationDto? pagination = null) 
        {
            Expression<Func<Vehicle, bool>>? filter = null;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var normalizedSearchTerm = searchTerm.Trim();

                filter = vehicle =>
                    vehicle.PlateNumber.Contains(normalizedSearchTerm) ||
                    vehicle.Brand.Contains(normalizedSearchTerm) ||
                    vehicle.Model.Contains(normalizedSearchTerm) ||
                    (vehicle.VIN != null && vehicle.VIN.Contains(normalizedSearchTerm)) ||
                    (vehicle.Customer != null &&
                     vehicle.Customer.FullName.Contains(normalizedSearchTerm));
            }

            var result = await _vehicleRepository.GetAllAsync(
                archiveStatus,
                sortOptions,
                pagination,
                filter,
                query => query.Include(vehicle => vehicle.Customer));

            return new PagedResultDto<VehicleDto>
            {
                Items = result.Items.Select(MapToDto),
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                TotalPages = result.TotalPages
            };
        }

        public async Task<VehicleDto?> GetVehicleByIdAsync(int id)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(
                id,
                query => query.Include(vehicle => vehicle.Customer));

            if (vehicle == null)
            {
                return null;
            }

            return MapToDto(vehicle);
        }

        public async Task<VehicleDto> CreateVehicleAsync(CreateVehicleDto createVehicleDto)
        {
            await ValidateUniquePlateNumberAsync(createVehicleDto.PlateNumber);
            await ValidateCustomerAsync(createVehicleDto.CustomerId);
            ValidateVehicleYear(createVehicleDto.Year);

            var vehicle = new Vehicle
            {
                PlateNumber = createVehicleDto.PlateNumber,
                Brand = createVehicleDto.Brand,
                Model = createVehicleDto.Model,
                Year = createVehicleDto.Year,
                Color = createVehicleDto.Color,
                VIN = createVehicleDto.VIN,
                CurrentMileage = createVehicleDto.CurrentMileage,
                CustomerId = createVehicleDto.CustomerId,
            };

            await _vehicleRepository.AddAsync(vehicle);
            await _unitOfWork.SaveChangesAsync();
            return MapToDto(vehicle);
        }

        public async Task<VehicleDto> UpdateVehicleAsync(int id, UpdateVehicleDto updateVehicleDto)
        {
            var vehicle = await ValidateVehicleAsync(id);

            if (vehicle.PlateNumber != updateVehicleDto.PlateNumber)
            {
                await ValidateUniquePlateNumberAsync(updateVehicleDto.PlateNumber, id);
            }

            await ValidateCustomerAsync(updateVehicleDto.CustomerId);

            ValidateVehicleYear(updateVehicleDto.Year);

            vehicle.PlateNumber = updateVehicleDto.PlateNumber;
            vehicle.Brand = updateVehicleDto.Brand;
            vehicle.Model = updateVehicleDto.Model;
            vehicle.Year = updateVehicleDto.Year;
            vehicle.Color = updateVehicleDto.Color;
            vehicle.VIN = updateVehicleDto.VIN;
            vehicle.CurrentMileage = updateVehicleDto.CurrentMileage;
            vehicle.CustomerId = updateVehicleDto.CustomerId;

            await _vehicleRepository.UpdateAsync(
                vehicle,
                updateVehicleDto.RowVersion);
            await _unitOfWork.SaveChangesAsync();
            return MapToDto(vehicle);
        }

        public async Task ArchiveVehicleAsync(int id, byte[] rowVersion)
        {
            var vehicle = await _vehicleRepository.GetByIdIncludingArchivedAsync(id);

            if (vehicle == null)
            {
                throw new NotFoundException($"Vehicle with ID {id} not found.");
            }

            if (vehicle.IsArchived)
            {
                throw new ValidationException($"Vehicle with ID {id} is already archived.");
            }
            await _vehicleRepository.ArchiveAsync(vehicle, rowVersion);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UnarchiveVehicleAsync(int id, byte[] rowVersion)
        {
            var vehicle = await _vehicleRepository.GetByIdIncludingArchivedAsync(id);

            if (vehicle == null)
            {
                throw new NotFoundException($"Vehicle with ID {id} not found.");
            }

            if (!vehicle.IsArchived)
            {
                throw new ValidationException($"Vehicle {id} is not archived.");
            }

            await _vehicleRepository.UnarchiveAsync(vehicle, rowVersion);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<VehicleDto> ReassignVehicleAsync(int vehicleId, ReassignVehicleDto reassignVehicleDto)
        {
            var vehicle = await ValidateVehicleAsync(vehicleId);
            await ValidateCustomerAsync(reassignVehicleDto.CustomerId);

            vehicle.CustomerId = reassignVehicleDto.CustomerId;

            await _vehicleRepository.UpdateAsync(vehicle);
            await _unitOfWork.SaveChangesAsync();
            return MapToDto(vehicle);
        }

        private static VehicleDto MapToDto(Vehicle vehicle)
        {
            return new VehicleDto
            {
                Id = vehicle.Id,
                PlateNumber = vehicle.PlateNumber,
                Brand = vehicle.Brand,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Color = vehicle.Color,
                VIN = vehicle.VIN,
                CurrentMileage = vehicle.CurrentMileage,
                CustomerId = vehicle.CustomerId,

                Customer = vehicle.Customer == null
                    ? null
                    : new CustomerSummaryDto
                    {
                        Id = vehicle.Customer.Id,
                        FullName = vehicle.Customer.FullName,
                        IsArchived = vehicle.Customer.IsArchived
                    },

                CreatedAt = vehicle.CreatedAt,
                UpdatedAt = vehicle.UpdatedAt,
                IsArchived = vehicle.IsArchived,
                RowVersion = vehicle.RowVersion
            };
        }

        private async Task<Vehicle> ValidateVehicleAsync(int vehicleId)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);

            if (vehicle == null)
            {
                throw new NotFoundException($"Vehicle with ID {vehicleId} not found.");
            }

            return vehicle;
        }

        private async Task ValidateUniquePlateNumberAsync(string plateNumber, int? currentVehicleId = null)
        {
            var existingVehicle = await _vehicleRepository.GetByPlateNumberAsync(plateNumber);

            if (existingVehicle != null && existingVehicle.Id != currentVehicleId)
            {
                throw new ValidationException("A vehicle with this plate number already exists.");
            }
        }

        private static void ValidateVehicleYear(int year)
        {
            var currentYear = DateTime.UtcNow.Year;

            if (year < 1900 || year > currentYear + 1)
            {
                throw new ValidationException($"The vehicle year must be between 1900 and {currentYear + 1}.");
            }
        }

        private async Task<Customer?> ValidateCustomerAsync(int? customerId)
        {
            if (!customerId.HasValue)   
            {
                return null;
            }

            var customer = await _customerRepository.GetByIdAsync(customerId.Value);

            if (customer == null)
            {
                throw new NotFoundException($"Customer with ID {customerId} not found.");
            }

            if (customer.IsArchived)
            {
                throw new ValidationException($"Customer with ID {customerId} is archived.");
            }

            return customer;
        }
    }
}
