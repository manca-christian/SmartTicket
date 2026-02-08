using System.Net.Http;

namespace SmartTicket.IntegrationTests;

public sealed class PassThroughHandler : DelegatingHandler
{
    public PassThroughHandler(HttpMessageHandler innerHandler)
    {
        InnerHandler = innerHandler;
    }
}
