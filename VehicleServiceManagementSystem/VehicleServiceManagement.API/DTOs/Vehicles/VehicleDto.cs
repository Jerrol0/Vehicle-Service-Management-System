using VehicleServiceManagement.API.DTOs.Customers;

namespace VehicleServiceManagement.API.DTOs.Vehicles
{
    public class VehicleDto
    {
        public required int Id { get; set; }
        public required string PlateNumber { get; set; }
        public required string Brand { get; set; }
        public required string Model { get; set; }
        public required int Year { get; set; }
        public required string Color { get; set; }
        public string? VIN { get; set; }
        public required int CurrentMileage { get; set; }

        public int? CustomerId { get; set; }
        public CustomerSummaryDto? Customer { get; set; }
        
        public required DateTime CreatedAt { get; set; }
        public required DateTime UpdatedAt { get; set; }

        public required bool IsArchived { get; set; }

        public required byte[] RowVersion { get; set; }
    }
}
