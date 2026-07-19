using VehicleServiceManagement.API.Enums;

namespace VehicleServiceManagement.API.DTOs.ServiceRecords
{
    public class ServiceRecordFilterDto
    {   
        public int? VehicleId { get; set; }
        public ServiceType? ServiceType { get; set; }
        public ServiceStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
