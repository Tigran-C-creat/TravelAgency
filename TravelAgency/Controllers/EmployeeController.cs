using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using TravelAgency.Application.DTOs.Response;
using TravelAgency.Application.Queries.GetEmployee;
using TravelAgency.Domain.Enums;


namespace TravelAgency.Controllers
{
    /// <summary>
    /// Контроллер для работы с Employee
    /// </summary>
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
        /// Возвращает Employee по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор Employee</param>
        [Authorize(Roles = TravelAgencyRole.ReadOrWrite)]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EmployeeDto), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            return Ok(await _mediator.Send(new GetEmployeeQuery(id)));
        }
    }
}
