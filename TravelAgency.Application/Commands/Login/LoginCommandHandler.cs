using MediatR;
using TravelAgency.Application.DTOs.Response;
using TravelAgency.Application.Interfaces;
using TravelAgency.Domain.Interfaces;
using TravelAgency.Domain.Enums;
using TravelAgency.Shared.Exeptions;



namespace TravelAgency.Application.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResult>
    {
        private readonly IJwtTokenGenerator _tokenGenerator;
        private readonly IRepository _repository;
        private readonly IPasswordService _passwordService;

        public LoginCommandHandler(IJwtTokenGenerator tokenGenerator, IRepository repository, IPasswordService passwordService)
        {
            _tokenGenerator = tokenGenerator;
            _repository = repository;
            _passwordService = passwordService;
        }

        public async Task<LoginResult> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var employee = await _repository.GetEmployeeByLoginAsync(request.Username, cancellationToken);
            if (employee != null)
            {
                if (employee.Status != EmployeeStatus.Active || !PasswordMatches(request.Password, employee.Password, out var legacyPassword))
                    throw new LogicException("Invalid username or password.") { StatusCode = System.Net.HttpStatusCode.Unauthorized };

                if (legacyPassword)
                {
                    employee.Password = _passwordService.Hash(request.Password);
                    await _repository.SaveChangesAsync(cancellationToken);
                }

                var roles = new List<string> { TravelAgencyRole.Read, TravelAgencyRole.Write };
                return Result(request.Username, roles, _tokenGenerator.GenerateToken(request.Username, roles));
            }

            var client = await _repository.GetClientByLoginAsync(request.Username, cancellationToken);
            if (client == null || !PasswordMatches(request.Password, client.Password, out var legacyClientPassword))
                throw new LogicException("Invalid username or password.") { StatusCode = System.Net.HttpStatusCode.Unauthorized };

            if (legacyClientPassword)
            {
                client.Password = _passwordService.Hash(request.Password);
                await _repository.SaveChangesAsync(cancellationToken);
            }

            var clientRoles = new List<string> { "Client" };
            return Result(request.Username, clientRoles,
                _tokenGenerator.GenerateToken(request.Username, clientRoles, client.Id));
        }

        private bool PasswordMatches(string password, string storedValue, out bool legacyPassword)
        {
            if (_passwordService.Verify(password, storedValue))
            {
                legacyPassword = false;
                return true;
            }

            // Migrate existing plaintext passwords to hashes at the next successful login.
            legacyPassword = storedValue == password;
            return legacyPassword;
        }

        private static LoginResult Result(string username, List<string> roles, string token) => new()
        {
            Token = token,
            Username = username,
            Roles = roles
        };
    }
}

