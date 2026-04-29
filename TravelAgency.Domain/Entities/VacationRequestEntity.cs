using System.ComponentModel.DataAnnotations.Schema;
using TravelAgency.Domain.Enums;


namespace TravelAgency.Domain.Entities
{
    public class VacationRequestEntity
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("vacation_id")]
        public Guid VacationId { get; set; }

        [Column("status")]
        public VacationRequestStatus Status { get; set; }

        [Column("message")]
        public string Message { get; set; } = null!;

        public VacationEntity Vacation { get; set; } = null!;
    }
}
