namespace TravelAgency.Application.DTOs.Response
{
    public class EmployeeDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Login { get; set; } = null!;
        public string Status { get; set; } = null!;
    }
}
