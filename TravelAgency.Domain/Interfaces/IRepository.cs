namespace TravelAgency.Domain.Interfaces
{
    /// <summary>
    /// Интерфейс репозитория.
    /// </summary>
    public interface IRepository
    {
        /// <summary>
        /// Поиск в репозитории или загрузка по id.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<T?> GetAsync<T>(Guid id, CancellationToken? cancellationToken = null) 
            where T : class;
    }
}
