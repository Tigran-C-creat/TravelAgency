namespace TravelAgency.Shared.Constants
{
    /// <summary>
    /// Ключи для кэширования данных.
    /// </summary>
    public static class CacheKeys
    {
        public const string AllVacationRequests = "vacation_requests:all";

        public static string Employee(Guid id) => $"employee:{id}";
        public static string Client(Guid id) => $"client:{id}";
    }
}