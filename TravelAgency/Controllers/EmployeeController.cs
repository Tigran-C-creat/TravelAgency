using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using TravelAgency.Application.Commands.Employee;
using TravelAgency.Application.DTOs.Response;
using TravelAgency.Application.Queries.GetEmployee;
using TravelAgency.Domain.Enums;


namespace TravelAgency.Controllers
{
    /// <summary>
    /// Контроллер для работы с Employee
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="mediator">медиатр</param>
        public EmployeeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Возвращает сотрудника по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор Employee</param>
        [Authorize(Roles = TravelAgencyRole.Read)]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EmployeeDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            return Ok(await _mediator.Send(new GetEmployeeQuery(id), cancellationToken));
        }

        /// <summary>
        /// Cоздает сотрудника
        /// </summary>
        [Authorize(Roles = TravelAgencyRole.Write)]
        [HttpPost]
        [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> CreateAsync([FromBody] CreateEmployeeCommand command, CancellationToken cancellationToken)
        {
            var employeeId = await _mediator.Send(command, cancellationToken);
            return Created(employeeId.ToString(), employeeId);
        }

        /// <summary>
        /// Обновляет status сотрудника
        /// </summary>
        [Authorize(Roles = TravelAgencyRole.Write)]
        [HttpPatch("{id}/status")]
        [ProducesResponseType((int)HttpStatusCode.NoContent)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> UpdateStatusAsync(Guid id, [FromBody] EmployeeStatus status, CancellationToken cancellationToken)
        {
            await _mediator.Send(new UpdateEmployeeStatusCommand
            {
                Id = id,
                Status = status
            }, cancellationToken);

            return NoContent();
        }
    }
}
