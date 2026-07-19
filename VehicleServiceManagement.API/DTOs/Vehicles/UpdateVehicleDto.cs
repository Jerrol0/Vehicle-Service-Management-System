using System.ComponentModel.DataAnnotations;

namespace VehicleServiceManagement.API.DTOs.Vehicles
{
    public class UpdateVehicleDto
    {
        [Required(ErrorMessage = "Plate number is required.")]
        [MaxLength(20, ErrorMessage = "Plate number cannot exceed 20 characters.")]
        public string PlateNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Brand is required.")]
        [MaxLength(50, ErrorMessage = "Brand cannot exceed 50 characters.")]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "Model is required.")]
        [MaxLength(50, ErrorMessage = "Model cannot exceed 50 characters.")]
        public string Model { get; set; } = string.Empty;

        // Service Layer enforce business rule
        [Required(ErrorMessage = "Year is required.")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Color is required.")]
        [MaxLength(30, ErrorMessage = "Color cannot exceed 30 characters.")]
        public string Color { get; set; } = string.Empty;

        [MaxLength(17, ErrorMessage = "VIN cannot exceed 17 characters.")]
        public string? VIN { get; set; }

        [Required(ErrorMessage = "Current mileage is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Current mileage cannot be negative.")]
        public int CurrentMileage { get; set; }

        public int? CustomerId { get; set; }
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
