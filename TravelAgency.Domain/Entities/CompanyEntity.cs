
using System.ComponentModel.DataAnnotations.Schema;

namespace TravelAgency.Domain.Entities
{
    public class CompanyEntity
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("name")]
        public string Name { get; set; } = null!;

        [Column("inn")]
        public string Inn { get; set; } = null!;

        [Column("legal_address")]
        public string LegalAddress { get; set; } = null!;

        public List<ClientEntity> Clients { get; set; } = new();
    }
}
