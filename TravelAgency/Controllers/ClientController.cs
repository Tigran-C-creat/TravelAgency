using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using TravelAgency.Application.Commands.Client;
using TravelAgency.Domain.Enums;

namespace TravelAgency.Controllers
{
    /// <summary>
    /// Контроллер для работы с клиентами.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly IMediator _mediator;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="mediator">медиатр</param>
        public ClientController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Cоздает клиента
        /// </summary>
        [Authorize(Roles = TravelAgencyRole.Write)]
        [HttpPost]
        [ProducesResponseType(typeof(Guid), (int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<IActionResult> CreateClient([FromBody] CreateClientCommand command, CancellationToken cancellationToken)
        {
            var clientid = await _mediator.Send(command, cancellationToken);
            return StatusCode(201, clientid); 
        }
    }
}
