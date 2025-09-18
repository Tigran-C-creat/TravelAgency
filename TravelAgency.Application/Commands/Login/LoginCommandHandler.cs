using MediatR;
using TravelAgency.Application.DTOs.Response;
using TravelAgency.Application.Interfaces;



namespace TravelAgency.Application.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
    {
        private readonly IJwtTokenGenerator _tokenGenerator;

        public LoginCommandHandler(IJwtTokenGenerator tokenGenerator)
        {
            _tokenGenerator = tokenGenerator;
        }

        public Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken) // CancellationToken узнать что это
        {
            var roles = request.Username == "admin"
                ? new List<string> { "Read", "Write" }
                : new List<string> { "Read" };

            var token = _tokenGenerator.GenerateToken(request.Username, roles);

            LoginResult loginResult = new()
            {
                Token = token,
                Username = request.Username,
                Roles = roles
            };
            return Task.FromResult(loginResult);
        }
    }
}

