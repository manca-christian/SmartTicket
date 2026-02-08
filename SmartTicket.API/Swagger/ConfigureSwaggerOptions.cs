using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SmartTicket.API.Swagger;

public sealed class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = "SmartTicket API",
                Version = description.ApiVersion.ToString(),
                Description = "Le operazioni di modifica (PUT/DELETE su ticket, assign, close, priority, due-date, comment) richiedono l'header If-Match con l'ETag ottenuto dal GET del ticket. Le richieste sono soggette a rate limiting (429 Too Many Requests)."
            });
        }

        options.DocInclusionPredicate((docName, apiDesc) =>
            apiDesc.GroupName == docName &&
            apiDesc.RelativePath?.StartsWith("api/v", StringComparison.OrdinalIgnoreCase) == true);

        options.OperationFilter<IfMatchHeaderOperationFilter>();
        options.OperationFilter<RateLimitResponseOperationFilter>();
        options.SchemaFilter<ExamplesSchemaFilter>();

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Inserisci: Bearer {il tuo JWT}"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    }
}
