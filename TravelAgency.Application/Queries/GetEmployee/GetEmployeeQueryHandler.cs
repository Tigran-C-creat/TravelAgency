using AutoMapper;
using MediatR;
using TravelAgency.Application.DTOs.Response;
using TravelAgency.Domain.Entities;
using TravelAgency.Domain.Interfaces;

namespace TravelAgency.Application.Queries.GetEmployee
{
    public class GetEmployeeQueryHandler : IRequestHandler<GetEmployeeQuery, EmployeeDto?>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;

        public GetEmployeeQueryHandler(IRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<EmployeeDto?> Handle(GetEmployeeQuery request, CancellationToken cancellationToken)
        {
            var employeeEntity = await _repository.GetOrThrowAsync<EmployeeEntity>(request.Id, cancellationToken);

            // Преобразуем сущность в dto
            var employeeDto = _mapper.Map<EmployeeDto>(employeeEntity);

            return employeeDto;
        }

    }
}
