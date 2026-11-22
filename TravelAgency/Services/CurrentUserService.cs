using System.Security.Claims;
using TravelAgency.Application.Interfaces;
using TravelAgency.Shared.Exeptions;
using System.Net;

namespace TravelAgency.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor) => _accessor = accessor;

    public Guid ClientId
    {
        get
        {
            var value = _accessor.HttpContext?.User.FindFirstValue("client_id");
            if (Guid.TryParse(value, out var id)) return id;
            throw new LogicException("Authenticated user is not a client.") { StatusCode = HttpStatusCode.Forbidden };
        }
    }
}
