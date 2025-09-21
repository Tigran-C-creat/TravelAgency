using System.Net;

namespace TravelAgency.Shared.Exeptions
{
    public class NotFoundException : LogicException
    {
        public NotFoundException()
        {
            StatusCode = HttpStatusCode.NotFound;
        }

        public NotFoundException(string message) : base(message)
        {
        }
    }
}
