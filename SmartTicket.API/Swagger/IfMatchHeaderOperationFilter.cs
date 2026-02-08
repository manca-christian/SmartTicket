using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SmartTicket.API.Swagger;

public sealed class IfMatchHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!context.MethodInfo.GetCustomAttributes(typeof(RequireIfMatchAttribute), inherit: true).Any())
            return;

        operation.Parameters ??= new List<OpenApiParameter>();

        if (operation.Parameters.Any(p => string.Equals(p.Name, "If-Match", StringComparison.OrdinalIgnoreCase)))
            return;

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "If-Match",
            In = ParameterLocation.Header,
            Required = true,
            Description = "ETag ottenuto dal GET del ticket.",
            Schema = new OpenApiSchema { Type = "string" }
        });
    }
}
