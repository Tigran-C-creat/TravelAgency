using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TravelAgency.Domain.Entities;

namespace TravelAgency.Persistence.Configurations
{
    public class ClientEntityConfiguration : IEntityTypeConfiguration<ClientEntity>
    {
        public void Configure(EntityTypeBuilder<ClientEntity> builder)
        {
            builder.ToTable("client");
            builder.HasKey(x => x.Id);

            // Client → Company (many-to-one)
            builder
                .HasOne(c => c.Company)
                .WithMany(c => c.Clients)
                .HasForeignKey(c => c.CompanyId);

            // Client ↔ Vacation (many-to-many)
            builder
                .HasMany(c => c.Vacations)
                .WithMany(v => v.Clients)
                .UsingEntity<Dictionary<string, object>>(
                    "vacation_clients",
                    j => j
                        .HasOne<VacationEntity>()
                        .WithMany()
                        .HasForeignKey("vacation_id")
                        .HasConstraintName("fk_vacation_clients_vacation_id")
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne<ClientEntity>()
                        .WithMany()
                        .HasForeignKey("client_id")
                        .HasConstraintName("fk_vacation_clients_client_id")
                        .OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.ToTable("vacation_clients");
                        j.HasKey("client_id", "vacation_id");
                    });
        }
    }
}
