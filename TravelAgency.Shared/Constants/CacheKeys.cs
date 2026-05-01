namespace TravelAgency.Shared.Constants
{
    public static class CacheKeys
    {
        public static string Employee(Guid id) => $"employee:{id}";
        public static string Client(Guid id) => $"client:{id}";
    }
}
