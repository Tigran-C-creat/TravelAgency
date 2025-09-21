using System.Net;

namespace TravelAgency.Shared.Exeptions
{
    public class LogicException : Exception
    {
        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.BadRequest;
        public string? Code { get; set; }

        public LogicException()
        {
        }

        public LogicException(string message) : base(message)
        {
        }

        public LogicException(string message, string code) : base(message)
        {
            Code = code;
        }

        public LogicException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public LogicException(string message, string code, Exception innerException) : base(message, innerException)
        {
            Code = code;
        }

    }
}
