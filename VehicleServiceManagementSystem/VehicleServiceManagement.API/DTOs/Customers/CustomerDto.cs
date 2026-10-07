namespace VehicleServiceManagement.API.DTOs.Customers
{
    public class CustomerDto
    {
        public required int Id { get; set; }
        public required string FullName { get; set; }
        public required string ContactNumber { get; set; }
        public required string Email { get; set; }
        public required string Address { get; set; }

        public required DateTime CreatedAt { get; set; }
        public required DateTime UpdatedAt { get; set; }

        public required bool IsArchived { get; set; }

        public required byte[] RowVersion { get; set; }
    }
}
