namespace SmartTicket.Application.Exceptions;

public sealed class AccountLockedException : Exception
{
    public DateTime UntilUtc { get; }

    public AccountLockedException(DateTime untilUtc, string? message = null)
        : base(message ?? "Account temporaneamente bloccato.")
    {
        UntilUtc = untilUtc;
    }
}
