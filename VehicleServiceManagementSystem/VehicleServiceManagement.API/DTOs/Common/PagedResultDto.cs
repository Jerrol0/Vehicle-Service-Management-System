namespace VehicleServiceManagement.API.DTOs.Common
{
    public class PagedResultDto<T>
    {
        public required IEnumerable<T> Items { get; set; }
        public required int PageNumber { get; set; }
        public required int PageSize { get; set; }
        public required int TotalCount { get; set; }
        public required int TotalPages { get; set; }
    }
}
