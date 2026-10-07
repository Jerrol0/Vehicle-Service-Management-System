using System.ComponentModel.DataAnnotations;

namespace VehicleServiceManagement.API.DTOs.Vehicles
{
    public class ReassignVehicleDto
    {
        [Required(ErrorMessage = "Customer ID is required.")]
        public int CustomerId { get; set; }
    }
}
