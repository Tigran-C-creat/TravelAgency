using AutoMapper;
using MediatR;
using TravelAgency.Application.DTOs.Response;
using TravelAgency.Domain.Entities;
using TravelAgency.Domain.Interfaces;
using TravelAgency.Shared.Constants;

namespace TravelAgency.Application.Queries.GetVacationRequest;

/// <summary>
/// Обработчик запроса для получения всех заявок на отпуск.
/// </summary>
public class GetVacationRequestsQueryHandler
    : IRequestHandler<GetVacationRequestsQuery, IReadOnlyList<VacationRequestDto>>
{
    private readonly IRepository _repository;
    private readonly IMapper _mapper;
    private readonly IRedisCacheService _cache;

    public GetVacationRequestsQueryHandler(IRepository repository, IMapper mapper, IRedisCacheService cache)
    {
        _repository = repository;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<IReadOnlyList<VacationRequestDto>> Handle(GetVacationRequestsQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.AllVacationRequests;

        return await _cache.TryGetOrSetAsync(
            cacheKey,
            async () =>
            {
                var vacationRequests = await _repository.GetAllAsync<VacationRequestEntity>(cancellationToken); 
                return _mapper.Map<IReadOnlyList<VacationRequestDto>>(vacationRequests);
            },
            TimeSpan.FromHours(10)
        );
    }
}