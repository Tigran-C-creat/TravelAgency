using MediatR;

namespace TravelAgency.Application.Commands.Client
{
    /// <summary>
    /// Команда для обновления клиента.
    /// </summary>
    public record UpdateClientCommand(
        Guid Id,
        string? FullName,
        string? PassportSeries,
        string? PassportNumber,
        string? Phone,
        string? Email,
        string? Login,
        string? Password,
        Guid? CompanyId
        ) : IRequest;
}
