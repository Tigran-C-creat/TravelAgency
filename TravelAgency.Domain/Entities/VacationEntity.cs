using System.ComponentModel.DataAnnotations.Schema;

namespace TravelAgency.Domain.Entities
{
    public class VacationEntity
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = null!;

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime EndDate { get; set; }

        [Column("total_cost")]
        public decimal TotalCost { get; set; }

        [Column("employee_id")]
        public Guid EmployeeId { get; set; }
        public EmployeeEntity Employee { get; set; } = null!;

        // Many-to-many: Vacation ↔ Clients
        public List<ClientEntity> Clients { get; set; } = new();
    }
}
