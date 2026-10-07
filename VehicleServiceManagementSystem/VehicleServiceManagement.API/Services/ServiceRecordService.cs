using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using VehicleServiceManagement.API.Common.Exceptions;
using VehicleServiceManagement.API.DTOs.Common;
using VehicleServiceManagement.API.DTOs.Customers;
using VehicleServiceManagement.API.DTOs.ServiceRecords;
using VehicleServiceManagement.API.Enums;
using VehicleServiceManagement.API.Models;
using VehicleServiceManagement.API.Repositories;
using VehicleServiceManagement.API.Repositories.Interfaces;
using VehicleServiceManagement.API.Services.Interfaces;

namespace VehicleServiceManagement.API.Services
{
    public class ServiceRecordService : IServiceRecordService
    {
        private readonly IServiceRecordRepository _serviceRecordRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ServiceRecordService(
            IServiceRecordRepository serviceRecordRepository, 
            IVehicleRepository vehicleRepository, 
            IUnitOfWork unitOfWork)
        {
            _serviceRecordRepository = serviceRecordRepository;
            _vehicleRepository = vehicleRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResultDto<ServiceRecordDto>> GetAllServiceRecordsAsync(
            string? searchTerm = null,
            int? vehicleId = null,
            ServiceType? serviceType = null,
            ServiceStatus? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            ArchiveStatus archiveStatus = ArchiveStatus.Active,
            SortOptionsDto? sortOptions = null,
            PaginationDto? pagination = null)
        {
            if (startDate.HasValue && endDate.HasValue && startDate.Value > endDate.Value)
            {
                throw new ValidationException(
                    "Start date cannot be later than end date.");
            }

            var normalizedSearchTerm = searchTerm?.Trim();

            Expression<Func<ServiceRecord, bool>> filter = serviceRecord =>
                (
                    string.IsNullOrEmpty(normalizedSearchTerm) ||
                    serviceRecord.ServiceTitle.Contains(normalizedSearchTerm) ||
                    serviceRecord.Description.Contains(normalizedSearchTerm) ||
                    serviceRecord.Vehicle.PlateNumber.Contains(normalizedSearchTerm) ||
                    serviceRecord.Vehicle.Brand.Contains(normalizedSearchTerm) ||
                    serviceRecord.Vehicle.Model.Contains(normalizedSearchTerm)
                )
                &&
                (!vehicleId.HasValue || serviceRecord.VehicleId == vehicleId.Value)
                &&
                (!serviceType.HasValue || serviceRecord.ServiceType == serviceType.Value)
                &&
                (!status.HasValue || serviceRecord.Status == status.Value)
                &&
                (!startDate.HasValue || serviceRecord.ServiceDate >= startDate.Value)
                &&
                (!endDate.HasValue || serviceRecord.ServiceDate <= endDate.Value);

            var result = await _serviceRecordRepository.GetAllAsync(
                archiveStatus,
                sortOptions,
                pagination,
                filter,
                query => query
                    .Include(serviceRecord => serviceRecord.Vehicle)
                    .ThenInclude(vehicle => vehicle.Customer));

            return new PagedResultDto<ServiceRecordDto>
            {
                Items = result.Items.Select(MapToDto),
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalCount = result.TotalCount,
                TotalPages = result.TotalPages
            };
        }

        public async Task<ServiceRecordDto?> GetServiceRecordByIdAsync(int id)
        {
            var serviceRecord = await _serviceRecordRepository.GetByIdAsync(
                id,
                query => query
                    .Include(serviceRecord => serviceRecord.Vehicle)
                    .ThenInclude(vehicle => vehicle.Customer));

            if (serviceRecord == null)
            {
                return null;
            }
            return MapToDto(serviceRecord);
        }

        public async Task<ServiceRecordDto> CreateServiceRecordAsync(CreateServiceRecordDto createServiceRecordDto)
        {
            var vehicle = await ValidateVehicleAsync(createServiceRecordDto.VehicleId);

            ValidateServiceRecordData(
                createServiceRecordDto.ServiceType,
                createServiceRecordDto.ServiceDate,
                createServiceRecordDto.LaborCost,
                createServiceRecordDto.PartsCost,
                createServiceRecordDto.MileageAtService);

            ValidateVehicleMileage(
                createServiceRecordDto.MileageAtService,
                vehicle.CurrentMileage);

            var serviceHistory = await _serviceRecordRepository.GetVehicleServiceHistoryAsync(createServiceRecordDto.VehicleId);

            var latestService = serviceHistory.FirstOrDefault();

            if (latestService != null && createServiceRecordDto.MileageAtService < latestService.MileageAtService)
            {
                throw new ValidationException($"Mileage cannot be less than the previous mileage of {latestService.MileageAtService}.");
            }

            var serviceRecord = new ServiceRecord
            {
                ServiceTitle = createServiceRecordDto.ServiceTitle,
                ServiceType = createServiceRecordDto.ServiceType,
                Description = createServiceRecordDto.Description,
                LaborCost = createServiceRecordDto.LaborCost,
                PartsCost = createServiceRecordDto.PartsCost,
                Status = ServiceStatus.Pending,
                ServiceDate = createServiceRecordDto.ServiceDate,
                MileageAtService = createServiceRecordDto.MileageAtService,
                VehicleId = createServiceRecordDto.VehicleId
            };

            await _serviceRecordRepository.AddAsync(serviceRecord);
            await _unitOfWork.SaveChangesAsync();
            return MapToDto(serviceRecord);
        }

        public async Task<ServiceRecordDto> UpdateServiceRecordAsync(int id, UpdateServiceRecordDto updateServiceRecordDto)
        {
            var serviceRecord = await ValidateServiceRecordAsync(id);
            var vehicle = await ValidateVehicleAsync(serviceRecord.VehicleId);

            if (!Enum.IsDefined(typeof(ServiceStatus), updateServiceRecordDto.Status))
            {
                throw new ValidationException("Invalid service status.");
            }

            ValidateServiceRecordData(
                updateServiceRecordDto.ServiceType,
                updateServiceRecordDto.ServiceDate,
                updateServiceRecordDto.LaborCost,
                updateServiceRecordDto.PartsCost,
                updateServiceRecordDto.MileageAtService);

            ValidateStatusTransition(
                serviceRecord.Status,
                updateServiceRecordDto.Status);

            ValidateVehicleMileage(
                updateServiceRecordDto.MileageAtService,
                vehicle.CurrentMileage);

            await ValidateMileageChronologyAsync(
                id,
                serviceRecord.VehicleId,
                updateServiceRecordDto.MileageAtService);

            serviceRecord.ServiceTitle= updateServiceRecordDto.ServiceTitle;
            serviceRecord.ServiceType = updateServiceRecordDto.ServiceType;
            serviceRecord.Description = updateServiceRecordDto.Description;
            serviceRecord.LaborCost = updateServiceRecordDto.LaborCost;
            serviceRecord.PartsCost = updateServiceRecordDto.PartsCost;
            serviceRecord.Status = updateServiceRecordDto.Status;
            serviceRecord.ServiceDate = updateServiceRecordDto.ServiceDate;
            serviceRecord.MileageAtService = updateServiceRecordDto.MileageAtService;

            await _serviceRecordRepository.UpdateAsync(
                serviceRecord,
                updateServiceRecordDto.RowVersion);
            await _unitOfWork.SaveChangesAsync();
            return MapToDto(serviceRecord);
        }

        public async Task ArchiveServiceRecordAsync(int id, byte[] rowVersion)
        {
            var serviceRecord = await _serviceRecordRepository.GetByIdIncludingArchivedAsync(id);

            if (serviceRecord == null)
            {
                throw new NotFoundException($"Service record with ID {id} not found.");
            }

            if (serviceRecord.IsArchived)
            {
                throw new ValidationException($"Service Record with ID {id} is already archived.");
            }
            await _serviceRecordRepository.ArchiveAsync(serviceRecord, rowVersion);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UnarchiveServiceRecordAsync(int id, byte[] rowVersion)
        {
            var serviceRecord = await _serviceRecordRepository.GetByIdIncludingArchivedAsync(id);

            if (serviceRecord == null)
            {
                throw new NotFoundException($"Service Record with ID {id} not found.");
            }
            if (!serviceRecord.IsArchived)
            {
                throw new ValidationException($"Service Record with ID {id} is not archived.");
            }

            await _serviceRecordRepository.UnarchiveAsync(serviceRecord, rowVersion);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<ServiceRecordDto>> GetVehicleServiceHistoryAsync(int vehicleId)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);

            if (vehicle == null)
            {
                throw new NotFoundException($"Vehicle with ID {vehicleId} not found.");
            }
            var serviceHistory = await _serviceRecordRepository.GetVehicleServiceHistoryAsync(vehicleId);
            return serviceHistory.Select(MapToDto);
        }

        private static ServiceRecordDto MapToDto(ServiceRecord serviceRecord)
        {
            return new ServiceRecordDto
            {
                Id = serviceRecord.Id,
                ServiceTitle = serviceRecord.ServiceTitle,
                ServiceType = serviceRecord.ServiceType.ToString(),
                Description = serviceRecord.Description,
                LaborCost = serviceRecord.LaborCost,
                PartsCost = serviceRecord.PartsCost,
                TotalCost = serviceRecord.TotalCost,
                Status = serviceRecord.Status.ToString(),
                ServiceDate = serviceRecord.ServiceDate,
                MileageAtService = serviceRecord.MileageAtService,
                VehicleId = serviceRecord.VehicleId,
                Vehicle = serviceRecord.Vehicle == null
                    ? null
                    : new ServiceRecordVehicleDto
                    {
                        Id = serviceRecord.Vehicle.Id,
                        PlateNumber = serviceRecord.Vehicle.PlateNumber,
                        Brand = serviceRecord.Vehicle.Brand,
                        Model = serviceRecord.Vehicle.Model,
                        Year = serviceRecord.Vehicle.Year,
                        Color = serviceRecord.Vehicle.Color,
                        VIN = serviceRecord.Vehicle.VIN,
                        CurrentMileage = serviceRecord.Vehicle.CurrentMileage,
                        IsArchived = serviceRecord.Vehicle.IsArchived,
                        Customer = serviceRecord.Vehicle.Customer == null
                            ? null
                            : new CustomerSummaryDto
                            {
                                Id = serviceRecord.Vehicle.Customer.Id,
                                FullName = serviceRecord.Vehicle.Customer.FullName,
                                IsArchived = serviceRecord.Vehicle.Customer.IsArchived
                            }
                    },
                CreatedAt = serviceRecord.CreatedAt,
                UpdatedAt = serviceRecord.UpdatedAt,
                IsArchived = serviceRecord.IsArchived,
                RowVersion = serviceRecord.RowVersion
            };
        }

        private async Task<ServiceRecord> ValidateServiceRecordAsync(int serviceRecordId)
        {
            var serviceRecord = await _serviceRecordRepository.GetByIdAsync(serviceRecordId);

            if (serviceRecord == null)
            {
                throw new NotFoundException($"Service record with ID {serviceRecordId} not found.");
            }

            return serviceRecord;
        }

        private async Task<Vehicle> ValidateVehicleAsync(int vehicleId)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);

            if (vehicle == null)
            {
                throw new NotFoundException($"Vehicle with ID {vehicleId} not found.");
            }

            if (vehicle.IsArchived)
            {
                throw new ValidationException($"Vehicle with ID {vehicleId} is archived.");
            }

            return vehicle;
        }

        private static void ValidateServiceRecordData(
            ServiceType serviceType,
            DateTime serviceDate,
            decimal laborCost,
            decimal partsCost,
            int mileageAtService)
        {
            if (!Enum.IsDefined(typeof(ServiceType), serviceType))
            {
                throw new ValidationException("Invalid service type.");
            }

            if (serviceDate.Date > DateTime.UtcNow.Date)
            {
                throw new ValidationException("Service date cannot be in the future.");
            }

            if (laborCost < 0)
            {
                throw new ValidationException("Labor cost cannot be negative.");
            }

            if (partsCost < 0)
            {
                throw new ValidationException("Parts cost cannot be negative.");
            }

            if (mileageAtService < 0)
            {
                throw new ValidationException("Mileage at service cannot be negative.");
            }
        }

        private static void ValidateStatusTransition(
            ServiceStatus currentStatus,
            ServiceStatus newStatus)
        {
            if (currentStatus == newStatus)
            {
                return;
            }

            bool isValid =
                (currentStatus == ServiceStatus.Pending &&
                 newStatus == ServiceStatus.InProgress)

                ||

                (currentStatus == ServiceStatus.InProgress &&
                 newStatus == ServiceStatus.Completed); 
            
            if (!isValid)
            {
                throw new ValidationException(
                    $"Invalid status transition from {currentStatus} to {newStatus}.");
            }
        }

        private static void ValidateVehicleMileage(
            int mileageAtService,
            int currentMileage)
        {
            if (mileageAtService > currentMileage)
            {
                throw new ValidationException($"Mileage at service cannot exceed vehicle current mileage ({currentMileage}).");
            }
        }

        private async Task ValidateMileageChronologyAsync(
            int serviceRecordId,
            int vehicleId,
            int mileageAtService)
        {
            var serviceHistory = (await _serviceRecordRepository
                .GetVehicleServiceHistoryAsync(vehicleId))
                .OrderBy(sr => sr.ServiceDate)
                .ToList();

            var currentIndex = serviceHistory.FindIndex(sr => sr.Id == serviceRecordId);
            var previousService = currentIndex > 0
                ? serviceHistory[currentIndex - 1]
                : null;

            var nextService = currentIndex < serviceHistory.Count - 1
                ? serviceHistory[currentIndex + 1]
                : null;

            if (previousService != null &&
                mileageAtService < previousService.MileageAtService)
            {
                throw new ValidationException($"Mileage cannot be less than previous service mileage ({previousService.MileageAtService}).");
            }

            if (nextService != null &&
                mileageAtService > nextService.MileageAtService)
            {
                throw new ValidationException($"Mileage cannot exceed next service mileage ({nextService.MileageAtService})");
            }
        }
    }
}
