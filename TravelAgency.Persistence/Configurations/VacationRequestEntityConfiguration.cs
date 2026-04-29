using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelAgency.Domain.Entities;

namespace TravelAgency.Persistence.Configurations
{
    public class VacationRequestEntityConfiguration : IEntityTypeConfiguration<VacationRequestEntity>
    {
        public void Configure(EntityTypeBuilder<VacationRequestEntity> builder)
        {
            builder.ToTable("vacation_request");
            builder.HasKey(x => x.Id);

            // VacationRequest → Vacation (many-to-one)
            builder
                .HasOne(vr => vr.Vacation)
                .WithMany() // если у Vacation нет коллекции запросов
                .HasForeignKey(vr => vr.VacationId);
        }
    }
}
