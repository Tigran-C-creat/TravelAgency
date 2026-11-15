using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelAgency.Application.DTOs.Response;
using TravelAgency.Application.Queries.GetVacationRequest;
using TravelAgency.Domain.Enums;

namespace TravelAgency.Controllers;

/// <summary>
/// Контроллер для работы с заявками на отпуск.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class VacationRequestController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Конструктор.
    /// </summary>
    /// <param name="mediator">медиатр</param>
    public VacationRequestController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Получает все заявки на отпуск.
    /// </summary>
    [Authorize(Roles = TravelAgencyRole.Read)]
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<VacationRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllAsync(CancellationToken cancellationToken)
    {
        var requests = await _mediator.Send(
            new GetVacationRequestsQuery(), cancellationToken);

        return Ok(requests);
    }
}