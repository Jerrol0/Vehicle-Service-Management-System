using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VehicleServiceManagement.API.Enums;

namespace VehicleServiceManagement.API.Models
{
    public class ServiceRecord : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string ServiceTitle { get; set; } = string.Empty;

        [Required]
        public ServiceType ServiceType { get; set; }

        [Required]
        [MaxLength (500)]
        public string Description { get; set; } = string.Empty;

        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal LaborCost { get; set; }

        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PartsCost { get; set; }

        [NotMapped]
        public decimal TotalCost => LaborCost + PartsCost;

        [Required]
        public ServiceStatus Status { get; set; }

        [Required]
        public DateTime ServiceDate { get; set; }

        [Required]
        public int MileageAtService { get; set; }

        [Required]
        public int VehicleId { get; set; }
        public virtual Vehicle Vehicle { get; set; } = null!;
    }
}
