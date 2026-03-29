using MediatR;
using TravelAgency.Domain.Enums;

namespace TravelAgency.Application.Commands.Employee
{
    /// <summary>
    /// Команда создания сотрудника.
    /// </summary>
    public class CreateEmployeeCommand : IRequest<Guid>
    {
        public string FullName { get; set; } = null!;

        public string Login { get; set; } = null!;

        public string Password { get; set; } = null!;

        public EmployeeStatus Status { get; set; }
    }
}
