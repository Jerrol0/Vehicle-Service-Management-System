using Microsoft.EntityFrameworkCore;
using VehicleServiceManagement.API.Models;

namespace VehicleServiceManagement.API.Infrastructure
{
    public class VehicleServiceDbContext : DbContext
    {
        public VehicleServiceDbContext(DbContextOptions<VehicleServiceDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<ServiceRecord> ServiceRecords { get; set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            ConfigureCustomerEntity(modelBuilder);
            ConfigureVehicleEntity(modelBuilder);
            ConfigureServiceRecordEntity(modelBuilder);
            ConfigureConcurrencyTokens(modelBuilder);
        }   

        private void ConfigureCustomerEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Customer>(entity =>
            {
                // Table Name
                entity.ToTable("Customers");

                // Primary Key
                entity.HasKey(e => e.Id);

                // Property configurations
                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.ContactNumber)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Address)
                    .IsRequired()
                    .HasMaxLength(200);

                // Configure default values for CreatedAt and UpdatedAt
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("GETDATE()");

                // Index for faster queries
                entity.HasIndex(e => e.IsArchived);
            });
        } 

        private void ConfigureVehicleEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Vehicle>(entity =>
            {
                // Table Name
                entity.ToTable("Vehicles");

                // Primary Key
                entity.HasKey(e => e.Id);

                // Property configurations
                entity.Property(e => e.PlateNumber)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.Brand)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Model)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Color)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(e => e.VIN)
                    .HasMaxLength(17);

                // Configure default values for CreatedAt and UpdatedAt
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("GETDATE()");

                // Indexes for faster queries
                entity.HasIndex(e => e.PlateNumber)
                    .IsUnique();

                entity.HasIndex(e => e.CustomerId);
                entity.HasIndex(e => e.IsArchived);

                // Configure Relationships
                entity.HasOne(e => e.Customer)
                    .WithMany(c => c.Vehicles)
                    .HasForeignKey(e => e.CustomerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private void ConfigureServiceRecordEntity(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ServiceRecord>(entity =>
            {
                // Table Name
                entity.ToTable("ServiceRecords");

                // Primary Key
                entity.HasKey(e => e.Id);

                // Property configurations
                entity.Property(e => e.ServiceTitle)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.LaborCost)
                    .IsRequired()
                    .HasColumnType("decimal(10,2)");

                entity.Property(e => e.PartsCost)
                    .IsRequired()
                    .HasColumnType("decimal(10,2)");

                entity.Ignore(e => e.TotalCost);

                entity.Property(e => e.ServiceDate)
                    .IsRequired();

                entity.Property(e => e.MileageAtService)
                    .IsRequired();

                // Configure default values for CreatedAt and UpdatedAt
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETDATE()");
                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("GETDATE()");

                // Indexes for faster queries
                entity.HasIndex(e => e.VehicleId);
                entity.HasIndex(e => e.ServiceDate);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.ServiceType);
                entity.HasIndex(e => e.IsArchived); 

                // Configure Relationships
                entity.HasOne(e => e.Vehicle)
                    .WithMany(v => v.ServiceRecords)
                    .HasForeignKey(e => e.VehicleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }

        private static void ConfigureConcurrencyTokens(ModelBuilder modelBuilder)
        {
            var entityTypes = modelBuilder.Model
                .GetEntityTypes()
                .Where(entityType => typeof(BaseEntity).IsAssignableFrom(entityType.ClrType));

            foreach (var entityType in entityTypes)
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(BaseEntity.RowVersion))
                    .IsRowVersion();
            }
        }
    }
}
 