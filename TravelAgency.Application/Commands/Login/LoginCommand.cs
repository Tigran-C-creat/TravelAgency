using MediatR;
using TravelAgency.Application.DTOs.Response;

namespace TravelAgency.Application.Commands.Login
{
    public class LoginCommand : IRequest<LoginResult>
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
