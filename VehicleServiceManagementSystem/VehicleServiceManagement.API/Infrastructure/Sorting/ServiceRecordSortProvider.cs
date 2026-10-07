using System.Linq.Expressions;
using VehicleServiceManagement.API.Enums;
using VehicleServiceManagement.API.Models;

namespace VehicleServiceManagement.API.Infrastructure.Sorting
{
    public class ServiceRecordSortProvider : ISortExpressionProvider<ServiceRecord>
    {
        public IReadOnlyDictionary<string, Expression<Func<ServiceRecord, object>>> SortExpressions
            => new Dictionary<string, Expression<Func<ServiceRecord, object>>>
            {
                ["servicetitle"] = serviceRecord => serviceRecord.ServiceTitle,
                ["servicetype"] = serviceRecord =>
                    serviceRecord.ServiceType == ServiceType.Diagnostic ? 1 :
                    serviceRecord.ServiceType == ServiceType.Inspection ? 2 :
                    serviceRecord.ServiceType == ServiceType.Maintenance ? 3 :
                    serviceRecord.ServiceType == ServiceType.Repair ? 4 :
                    5,
                ["status"] = serviceRecord => serviceRecord.Status,
                ["servicedate"] = serviceRecord => serviceRecord.ServiceDate,
                ["laborcost"] = serviceRecord => serviceRecord.LaborCost,
                ["partscost"] = serviceRecord => serviceRecord.PartsCost,
                ["totalcost"] = serviceRecord => serviceRecord.LaborCost + serviceRecord.PartsCost,
                ["mileageatservice"] = serviceRecord => serviceRecord.MileageAtService,
                ["createdat"] = serviceRecord => serviceRecord.CreatedAt,
                ["updatedat"] = serviceRecord => serviceRecord.UpdatedAt
            };
    }
}
