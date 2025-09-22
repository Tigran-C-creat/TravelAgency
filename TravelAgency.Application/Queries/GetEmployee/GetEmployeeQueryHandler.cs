using AutoMapper;
using MediatR;
using TravelAgency.Application.DTOs.Response;
using TravelAgency.Domain.Entities;
using TravelAgency.Domain.Interfaces;
using TravelAgency.Shared.Constants;

namespace TravelAgency.Application.Queries.GetEmployee
{
    public class GetEmployeeQueryHandler : IRequestHandler<GetEmployeeQuery, EmployeeDto?>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;
        private readonly IRedisCacheService _cache;

        public GetEmployeeQueryHandler(IRepository repository, IMapper mapper, IRedisCacheService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<EmployeeDto?> Handle(GetEmployeeQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = CacheKeys.Employee(request.Id);

            return await _cache.TryGetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var entity = await _repository.GetOrThrowAsync<EmployeeEntity>(request.Id, cancellationToken);
                    return _mapper.Map<EmployeeDto>(entity);
                },
                TimeSpan.FromMinutes(10)
            );


        }

    }
}


//при обновлении сотрудника не забыть инвалидацию кеша - await _cache.RemoveAsync($"employee:{employeeId}"); 
