using SmartTicket.Application.Observability;

namespace SmartTicket.API.Observability;

public sealed class CorrelationIdHandler : DelegatingHandler
{
    private const string HeaderName = "X-Correlation-Id";

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var correlationId = CorrelationContext.Current ?? Guid.NewGuid().ToString("N");

        if (!request.Headers.Contains(HeaderName))
        {
            request.Headers.Add(HeaderName, correlationId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
