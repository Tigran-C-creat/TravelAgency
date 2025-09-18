using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using TravelAgency.Application.Queries.GetEmployee;
using TravelAgency.Domain.Enums;
using TravelAgency.Domain.Models;

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
        /// Возвращает сайт по идентификатору
        /// </summary>
        /// <param name="id">Идентификатор сайта</param>
        [Authorize(Roles = TravelAgencyRole.ReadOrWrite)]
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EmployeeModel), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> GetAsync(Guid id)
        {
            return Ok(await _mediator.Send(new GetEmployeeQuery(id)));
        }
    }
}
