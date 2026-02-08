namespace SmartTicket.Application.Exceptions;

public sealed class ConcurrencyException : Exception
{
    public ConcurrencyException(string message = "Conflitto: la risorsa è stata modificata da un altro utente.")
        : base(message) { }
}
