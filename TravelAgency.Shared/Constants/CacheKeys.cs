namespace TravelAgency.Shared.Constants
{
    public static class CacheKeys
    {
        public static string Employee(Guid id) => $"employee:string:{id}";
    }
}
