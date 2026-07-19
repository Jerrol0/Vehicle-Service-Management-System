using System.Linq.Expressions;
using VehicleServiceManagement.API.Models;

namespace VehicleServiceManagement.API.Infrastructure.Sorting
{
    public class ServiceRecordSortProvider : ISortExpressionProvider<ServiceRecord>
    {
        public IReadOnlyDictionary<string, Expression<Func<ServiceRecord, object>>> SortExpressions
            => new Dictionary<string, Expression<Func<ServiceRecord, object>>>
            {
                ["servicetitle"] = serviceRecord => serviceRecord.ServiceTitle,
                ["servicetype"] = serviceRecord => serviceRecord.ServiceType,
                ["status"] = serviceRecord => serviceRecord.Status,
                ["servicedate"] = serviceRecord => serviceRecord.ServiceDate,
                ["laborcost"] = serviceRecord => serviceRecord.LaborCost,
                ["partscost"] = serviceRecord => serviceRecord.PartsCost,
                ["mileageatservice"] = serviceRecord => serviceRecord.MileageAtService,
                ["createdat"] = serviceRecord => serviceRecord.CreatedAt,
                ["updatedat"] = serviceRecord => serviceRecord.UpdatedAt
            };
    }
}
