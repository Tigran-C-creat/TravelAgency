using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelAgency.Domain.Entities;

namespace TravelAgency.Persistence.Configurations
{
    public class VacationEntityConfiguration : IEntityTypeConfiguration<VacationEntity>
    {
        public void Configure(EntityTypeBuilder<VacationEntity> builder)
        {
            builder.ToTable("vacation");
            builder.HasKey(x => x.Id);

            // Vacation → Employee (many-to-one)
            builder
                .HasOne(v => v.Employee)
                .WithMany() // если у Employee нет коллекции отпусков
                .HasForeignKey(v => v.EmployeeId);

            // Vacation ↔ Clients (many-to-many)
            builder
                .HasMany(v => v.Clients)
                .WithMany(c => c.Vacations)
                .UsingEntity(j => j.ToTable("vacation_clients"));
        }
    }
}
