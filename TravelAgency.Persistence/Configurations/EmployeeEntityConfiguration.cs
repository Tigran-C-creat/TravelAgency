using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelAgency.Domain.Entities;

namespace TravelAgency.Persistence.Configurations
{
    public class EmployeeEntityConfiguration : IEntityTypeConfiguration<EmployeeEntity>
    {
        public void Configure(EntityTypeBuilder<EmployeeEntity> builder)
        {
            builder.ToTable("employee");
            builder.HasKey(x => x.Id);
        }
    }

}


