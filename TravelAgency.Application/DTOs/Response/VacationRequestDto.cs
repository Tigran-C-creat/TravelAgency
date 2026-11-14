using TravelAgency.Domain.Enums;

namespace TravelAgency.Application.DTOs.Response;

/// <summary>
/// Данные заявки на отпуск.
/// </summary>
public record VacationRequestDto(
        Guid Id,
        Guid ClientId,
        Guid VacationId,
        VacationRequestStatus Status,
        string? Message
    );