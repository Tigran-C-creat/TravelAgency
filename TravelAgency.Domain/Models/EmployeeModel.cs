
namespace TravelAgency.Domain.Models
{
    public class EmployeeModel
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = null!;

        public string Login { get; set; } = null!;

        public string Password { get; set; } = null!;


        public string Status { get; set; } = null!;
    }
}
