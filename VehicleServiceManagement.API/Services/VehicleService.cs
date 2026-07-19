using VehicleServiceManagement.API.Models;
using VehicleServiceManagement.API.DTOs.Vehicles;
using VehicleServiceManagement.API.Repositories.Interfaces;
using VehicleServiceManagement.API.Services.Interfaces;
using VehicleServiceManagement.API.Common.Exceptions;
using VehicleServiceManagement.API.DTOs.Common;

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

        public async Task<IEnumerable<VehicleDto>> GetAllVehiclesAsync(
            bool includeArchived = false,
            SortOptionsDto? sortOptions = null,
            PaginationDto? pagination = null) 
        {
            var vehicles = await _vehicleRepository.GetAllAsync(
                includeArchived,
                sortOptions,
                pagination);

            return vehicles.Select(MapToDto);
        }

        public async Task<VehicleDto?> GetVehicleByIdAsync(int id)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);

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

        public async Task ArchiveVehicleAsync(int id)
        {
            var vehicle = await ValidateVehicleAsync(id);
            await _vehicleRepository.ArchiveAsync(vehicle);
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

        public async Task<IEnumerable<VehicleDto>> SearchVehiclesAsync(string searchTerm)
        {
            var vehicles = await _vehicleRepository.FindAsync(vehicle =>
                vehicle.PlateNumber.Contains(searchTerm) ||
                vehicle.Brand.Contains(searchTerm) ||
                vehicle.Model.Contains(searchTerm));

            return vehicles.Select(MapToDto);
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
                CreatedAt = vehicle.CreatedAt,
                UpdatedAt = vehicle.UpdatedAt,
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
