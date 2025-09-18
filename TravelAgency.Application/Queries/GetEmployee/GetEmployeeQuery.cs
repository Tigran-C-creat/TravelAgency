using MediatR;
using TravelAgency.Domain.Models;

namespace TravelAgency.Application.Queries.GetEmployee
{
    public class GetEmployeeQuery : IRequest<EmployeeModel>
    {
        /// <summary>
        /// параметр для запроса на получение одного сайта
        /// </summary>
        /// <param name="id">Идентификатор сайта</param>
        public GetEmployeeQuery(Guid id)
        {
            Id = id;
        }

        /// <summary>
        /// Идентификатор.
        /// </summary>
        public Guid Id { get; set; }
    }
}
