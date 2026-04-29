using Microsoft.EntityFrameworkCore;
using TravelAgency.Domain.Entities;
using TravelAgency.Persistence.Configurations;


namespace TravelAgency.Persistence
{
    public class TravelAgencyContext : DbContext
    {
        public DbSet<EmployeeEntity> Employees { get; set; } = null!;
        public DbSet<ClientEntity> Clients { get; set; } = null!;
        public DbSet<CompanyEntity> Companies { get; set; } = null!;
        public DbSet<VacationEntity> Vacations { get; set; } = null!;
        public DbSet<VacationRequestEntity> VacationRequests { get; set; } = null!;


        public TravelAgencyContext(DbContextOptions<TravelAgencyContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new EmployeeEntityConfiguration());
            modelBuilder.ApplyConfiguration(new ClientEntityConfiguration());
            modelBuilder.ApplyConfiguration(new CompanyEntityConfiguration());
            modelBuilder.ApplyConfiguration(new VacationEntityConfiguration());
            modelBuilder.ApplyConfiguration(new VacationRequestEntityConfiguration());
            base.OnModelCreating(modelBuilder);
        }
    }
}

