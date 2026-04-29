using System.ComponentModel.DataAnnotations.Schema;

namespace TravelAgency.Domain.Entities
{
    public class ClientEntity
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("fullname")]
        public string FullName { get; set; } = null!;
        
        [Column("passport_series")]
        public string PassportSeries { get; set; } = null!;

        [Column("passport_number")]
        public string PassportNumber { get; set; } = null!;

        [Column("phone")]
        public string Phone { get; set; } = null!;

        [Column("email")]
        public string Email { get; set; } = null!;

        [Column("login")]
        public string Login { get; set; } = null!;

        [Column("password")]
        public string Password { get; set; } = null!;

        [Column("company_id")]
        public Guid? CompanyId { get; set; }

        public CompanyEntity? Company { get; set; }

        public List<VacationEntity> Vacations { get; set; } = new();
    }
}
