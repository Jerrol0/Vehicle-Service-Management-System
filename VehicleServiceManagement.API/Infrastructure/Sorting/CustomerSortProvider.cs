using System.Linq.Expressions;
using VehicleServiceManagement.API.Models;

namespace VehicleServiceManagement.API.Infrastructure.Sorting
{
    public class CustomerSortProvider : ISortExpressionProvider<Customer>
    {
        public IReadOnlyDictionary<string, Expression<Func<Customer, object>>> SortExpressions 
            => new Dictionary<string, Expression<Func<Customer, object>>>
            {
                ["fullname"] = customer => customer.FullName,
                ["email"] = customer => customer.Email,
                ["createdat"] = customer => customer.CreatedAt,
                ["updatedat"] = customer => customer.UpdatedAt
            };
    }
}
