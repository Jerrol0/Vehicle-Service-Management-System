using System.Linq.Expressions;

namespace VehicleServiceManagement.API.Infrastructure.Sorting
{
    public interface ISortExpressionProvider<T>
    {
        IReadOnlyDictionary<string, Expression<Func<T, object>>> SortExpressions { get; }
    }
}
