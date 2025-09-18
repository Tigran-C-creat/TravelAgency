using Microsoft.EntityFrameworkCore;
using TravelAgency.Domain.Entities;
using TravelAgency.Persistence.Configurations;


namespace TravelAgency.Persistence
{
    public class TravelAgencyContext : DbContext
    {
        public DbSet<EmployeeEntity> Employees { get; set; } = null!;


        public TravelAgencyContext(DbContextOptions<TravelAgencyContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new EmployeeEntityConfiguration());
            base.OnModelCreating(modelBuilder);
        }
    }
}

