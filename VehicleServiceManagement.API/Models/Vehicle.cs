using System.ComponentModel.DataAnnotations;

namespace VehicleServiceManagement.API.Models
{
    public class Vehicle : BaseEntity
    {
        [Required]
        [MaxLength(20)]
        public string PlateNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Model { get; set; } = string.Empty;

        [Required]
        public int Year { get; set; }

        [Required]
        [MaxLength(30)]
        public string Color { get; set; } = string.Empty;

        [MaxLength(17)]
        public string? VIN { get; set; }

        [Required]
        public int CurrentMileage { get; set; }

        public int? CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }
        public virtual ICollection<ServiceRecord> ServiceRecords { get; set; } = new List<ServiceRecord>();
    }
}