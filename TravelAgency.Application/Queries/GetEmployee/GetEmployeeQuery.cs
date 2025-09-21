using MediatR;
using TravelAgency.Application.DTOs.Response;

namespace TravelAgency.Application.Queries.GetEmployee
{
    public class GetEmployeeQuery : IRequest<EmployeeDto>
    {
        /// <summary>
        /// параметр для запроса на получение одного employee
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
