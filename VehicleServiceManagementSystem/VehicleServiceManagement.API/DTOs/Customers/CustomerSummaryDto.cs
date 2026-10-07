namespace VehicleServiceManagement.API.DTOs.Customers
{
    public class CustomerSummaryDto
    {
        public required int Id { get; set; }
        public required string FullName { get; set; }
        public required bool IsArchived { get; set; }
    }
}
