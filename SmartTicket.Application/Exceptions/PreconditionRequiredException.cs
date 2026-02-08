namespace SmartTicket.Application.Exceptions;

public sealed class PreconditionRequiredException : Exception
{
    public PreconditionRequiredException(string message = "Header If-Match mancante.")
        : base(message) { }
}
