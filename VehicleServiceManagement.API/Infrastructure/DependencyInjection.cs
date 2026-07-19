using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Infrastructure.Sorting;
using VehicleServiceManagement.API.Repositories;
using VehicleServiceManagement.API.Repositories.Interfaces;
using VehicleServiceManagement.API.Services;
using VehicleServiceManagement.API.Services.Interfaces;
using VehicleServiceManagement.API.Models;

namespace VehicleServiceManagement.API.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Configure DbContext
            services.AddDbContext<VehicleServiceDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });

            // Register Generic Repository
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Register specific repositories
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IVehicleRepository, VehicleRepository>();
            services.AddScoped<IServiceRecordRepository, ServiceRecordRepository>();

            // Register Services
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IVehicleService, VehicleService>();
            services.AddScoped<IServiceRecordService, ServiceRecordService>();

            // Register Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Register Sort Providers
            services.AddScoped<ISortExpressionProvider<Customer>, CustomerSortProvider>();
            services.AddScoped<ISortExpressionProvider<Vehicle>, VehicleSortProvider>();
            services.AddScoped<ISortExpressionProvider<ServiceRecord>, ServiceRecordSortProvider>();

            return services;
        }
    }
}
