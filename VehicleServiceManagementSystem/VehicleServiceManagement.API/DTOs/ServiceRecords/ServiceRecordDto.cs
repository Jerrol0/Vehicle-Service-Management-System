namespace VehicleServiceManagement.API.DTOs.ServiceRecords
{
    public class ServiceRecordDto
    {
        public required int Id { get; set; }
        public required string ServiceTitle { get; set; } 
        public required string ServiceType { get; set; } 
        public required string Description { get; set; } 
        public required decimal LaborCost { get; set; }
        public required decimal PartsCost { get; set; }
        public required decimal TotalCost { get; set; }
        public required string Status { get; set; } 
        public required DateTime ServiceDate { get; set; }
        public required int MileageAtService { get; set; }

        public required int VehicleId { get; set; }
        public ServiceRecordVehicleDto? Vehicle { get; set; }

        public required DateTime CreatedAt { get; set; }
        public required DateTime UpdatedAt { get; set; }

        public required bool IsArchived { get; set; }

        public required byte[] RowVersion { get; set; }
    }
}
