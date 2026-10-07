using System.Linq.Expressions;
using VehicleServiceManagement.API.Models;

namespace VehicleServiceManagement.API.Infrastructure.Sorting
{
    public class VehicleSortProvider : ISortExpressionProvider<Vehicle>
    {
        public IReadOnlyDictionary<string, Expression<Func<Vehicle, object>>> SortExpressions
            => new Dictionary<string, Expression<Func<Vehicle, object>>>
            {
                ["platenumber"] = vehicle => vehicle.PlateNumber,
                ["brand"] = vehicle => vehicle.Brand,
                ["model"] = vehicle => vehicle.Model,
                ["year"] = vehicle => vehicle.Year,
                ["currentmileage"] = vehicle => vehicle.CurrentMileage,
                ["createdat"] = vehicle => vehicle.CreatedAt,
                ["updatedat"] = vehicle => vehicle.UpdatedAt
            };
    }
}
