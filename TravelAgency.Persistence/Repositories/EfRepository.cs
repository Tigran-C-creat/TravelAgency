using TravelAgency.Domain.Interfaces;

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
    }
}
