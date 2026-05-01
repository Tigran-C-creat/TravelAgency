using MediatR;
using TravelAgency.Application.DTOs.Response;

namespace TravelAgency.Application.Queries.GetClient
{
    public class GetClientByIdQuery : IRequest<ClientDto?>
    {
        /// <summary>
        /// параметр для запроса на получение одного клиента
        /// </summary>
        /// <param name="id">Идентификатор клиента</param>

        public GetClientByIdQuery(Guid id) 
        {
            Id = id;
        }

        /// <summary>
        /// Идентификатор.
        /// </summary>
        public Guid Id { get; set; }
    }
}
