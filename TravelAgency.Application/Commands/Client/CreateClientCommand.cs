using MediatR;

namespace TravelAgency.Application.Commands.Client
{
    /// <summary>
    /// Команда создания клиента.
    /// </summary>
    public record CreateClientCommand(
     string FullName,
     string PassportSeries,
     string PassportNumber,
     string Phone,
     string Email,
     string Login,
     string Password
 ) : IRequest<Guid>;
}
