using MediatR;

namespace TravelAgency.Application.Commands.Employee
{
    /// <summary>
    /// Команда создания employee.
    /// </summary>
    public class CreateEmployeeCommand : IRequest<Guid>
    {
        public string FullName { get; set; } = null!;

        public string Login { get; set; } = null!;

        public string Password { get; set; } = null!;

        public string Status { get; set; } = null!;
    }
}
