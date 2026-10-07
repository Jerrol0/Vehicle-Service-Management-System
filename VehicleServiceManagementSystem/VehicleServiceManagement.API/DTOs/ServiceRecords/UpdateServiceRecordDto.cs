using System.ComponentModel.DataAnnotations;
using VehicleServiceManagement.API.Enums;

namespace VehicleServiceManagement.API.DTOs.ServiceRecords
{
    public class UpdateServiceRecordDto
    {
        [Required(ErrorMessage = "Service title is required.")]
        [MaxLength(100, ErrorMessage = "Service title cannot exceed 100 characters.")]
        public string ServiceTitle { get; set; } = string.Empty;

        [Required(ErrorMessage = "Service type is required.")]
        public ServiceType ServiceType { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Labor cost cannot be negative.")]
        public decimal LaborCost { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Parts cost cannot be negative.")]
        public decimal PartsCost { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        public ServiceStatus Status { get; set; }

        [Required(ErrorMessage = "Service date is required.")]
        public DateTime ServiceDate { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Mileage cannot be negative.")]
        public int MileageAtService { get; set; }

        [Required(ErrorMessage = "Row version is required.")]
        [MinLength(1, ErrorMessage = "Row version cannot be empty.")]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
