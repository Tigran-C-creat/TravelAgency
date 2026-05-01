using AutoMapper;
using MediatR;
using TravelAgency.Application.DTOs.Response;
using TravelAgency.Domain.Entities;
using TravelAgency.Domain.Interfaces;
using TravelAgency.Shared.Constants;

namespace TravelAgency.Application.Queries.GetClient
{
    public class GetClientByIdQueryHandler : IRequestHandler<GetClientByIdQuery, ClientDto?>
    {
        private readonly IRepository _repository;
        private readonly IMapper _mapper;
        private readonly IRedisCacheService _cache;

        public GetClientByIdQueryHandler(IRepository repository, IMapper mapper, IRedisCacheService cache)
        {
            _repository = repository;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<ClientDto?> Handle(GetClientByIdQuery request, CancellationToken cancellationToken)
        {
            var cacheKey = CacheKeys.Client(request.Id);

            return await _cache.TryGetOrSetAsync(
                cacheKey,
                async () =>
                {
                    var client = await _repository.GetOrThrowAsync<ClientEntity>(request.Id, cancellationToken);
                    return _mapper.Map<ClientDto?>(client);
                },
                TimeSpan.FromHours(10)
            );
        }
    }
}
