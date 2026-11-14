using MediatR;
using TravelAgency.Application.DTOs.Response;

namespace TravelAgency.Application.Queries.GetVacationRequest;

/// <summary>
/// Запрос для получения всех заявок на отпуск.
/// </summary>
public record GetVacationRequestsQuery
    : IRequest<IReadOnlyList<VacationRequestDto>>;