using MediatR;

namespace TravelAgency.Application.Commands.VacationRequest
{
    /// <summary>
    /// Команда для создания заявки на отпуск.
    /// </summary>
public record CreateVacationRequestCommand(
    Guid VacationId,
    string? Message) : IRequest<Guid>;
}
