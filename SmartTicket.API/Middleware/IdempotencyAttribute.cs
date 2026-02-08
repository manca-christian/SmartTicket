using Microsoft.AspNetCore.Mvc.Filters;

namespace SmartTicket.API.Middleware;

[AttributeUsage(AttributeTargets.Method)]
public sealed class IdempotencyAttribute : Attribute, IFilterFactory
{
    public bool IsReusable => false;

    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        => serviceProvider.GetRequiredService<IdempotencyFilter>();
}
