namespace TravelAgency.Application.DTOs.Response
{
    public class LoginResult
    {
        public string Token { get; set; }
        public string Username { get; set; }
        public List<string> Roles { get; set; }
    }
}
