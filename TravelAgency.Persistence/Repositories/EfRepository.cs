using TravelAgency.Domain.Interfaces;
using TravelAgency.Shared.Exeptions;

namespace TravelAgency.Persistence.Repositories
{
    public class EfRepository : IRepository
    {
        private readonly TravelAgencyContext _context;
        public EfRepository(TravelAgencyContext context)
        {
            _context = context;
        }

        public async Task<T?> GetAsync<T>(Guid id, CancellationToken? cancellationToken = null) where T : class
        {
            var token = cancellationToken ?? CancellationToken.None;

            return await _context.Set<T>()
                .FindAsync(new object[] { id }, token);
        }

        public async Task<T> GetOrThrowAsync<T>(Guid id, CancellationToken? cancellationToken = null) where T : class
        {
            var token = cancellationToken ?? CancellationToken.None;
            var entity = await _context.Set<T>().FindAsync(new object[] { id }, token);

            if (entity == null)
                throw new NotFoundException($"{typeof(T).Name} with ID {id} not found.");

            return entity;
        }

        public async Task AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : class
        {
            await _context.Set<T>().AddAsync(entity, cancellationToken);
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
