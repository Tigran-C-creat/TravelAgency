using System.ComponentModel.DataAnnotations.Schema;

namespace TravelAgency.Domain.Entities
{
    public class EmployeeEntity
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("fullname")]
        public string FullName { get; set; } = null!;

        [Column("login")]
        public string Login { get; set; } = null!;


        [Column("password")]
        public string Password { get; set; } = null!;


        [Column("status")]
        public string Status { get; set; } = null!;

    }
}
