namespace SmartTicket.Application.Exceptions;

public sealed class PreconditionFailedException : Exception
{
    public PreconditionFailedException(string message = "Precondizione fallita: la risorsa è stata modificata.")
        : base(message) { }
}
