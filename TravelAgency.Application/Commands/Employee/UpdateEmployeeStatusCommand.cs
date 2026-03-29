using MediatR;
using TravelAgency.Domain.Enums;

namespace TravelAgency.Application.Commands.Employee
{
    /// <summary>
    /// Команда для обновления сотрудника.
    /// </summary>
    public class UpdateEmployeeStatusCommand : IRequest
    {
        public Guid Id { get; set; }
        public EmployeeStatus Status { get; set; }
    }
}
