using System.ComponentModel.DataAnnotations;

namespace VehicleServiceManagement.API.DTOs.Common
{
    public class ArchiveRequestDto
    {
        [Required(ErrorMessage = "Row version is required.")]
        [MinLength(1, ErrorMessage = "Row version cannot be empty.")]
        public byte[] RowVersion { get; set; } = [];
    }
}
