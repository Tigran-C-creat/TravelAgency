namespace TravelAgency.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(string username, List<string> roles);

        string GenerateToken(string username, List<string> roles, Guid clientId);
    }
}
