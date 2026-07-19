namespace VehicleServiceManagement.API.DTOs.ServiceRecords
{
    public class ServiceRecordDto
    {
        public int Id { get; set; }
        public string ServiceTitle { get; set; } = string.Empty;
        public string ServiceType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal LaborCost { get; set; }
        public decimal PartsCost { get; set; }
        public decimal TotalCost { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime ServiceDate { get; set; }
        public int MileageAtService { get; set; }
        public int VehicleId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
