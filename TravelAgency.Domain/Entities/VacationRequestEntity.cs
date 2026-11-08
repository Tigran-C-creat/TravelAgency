using System.ComponentModel.DataAnnotations.Schema;
using TravelAgency.Domain.Enums;


namespace TravelAgency.Domain.Entities
{
    public class VacationRequestEntity
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("client_id")]
        public Guid ClientId { get; set; }

        [Column("vacation_id")]
        public Guid VacationId { get; set; }

        [Column("status")]
        public VacationRequestStatus Status { get; set; } = VacationRequestStatus.New;

        [Column("message")]
        public string? Message { get; set; }

        public VacationEntity Vacation { get; set; } = null!;

        public ClientEntity Client { get; set; } = null!;
    }
}
