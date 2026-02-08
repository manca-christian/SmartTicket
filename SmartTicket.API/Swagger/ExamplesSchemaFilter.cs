using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using SmartTicket.Application.DTOs.Tickets;
using SmartTicket.Domain.Enums;

namespace SmartTicket.API.Swagger;

public sealed class ExamplesSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(CreateTicketDto))
        {
            schema.Example = new OpenApiObject
            {
                ["title"] = new OpenApiString("Problema accesso"),
                ["description"] = new OpenApiString("Non riesco ad accedere al sistema")
            };
            return;
        }

        if (context.Type == typeof(UpdateTicketDto))
        {
            schema.Example = new OpenApiObject
            {
                ["title"] = new OpenApiString("Aggiornamento titolo"),
                ["description"] = new OpenApiString("Dettagli aggiornati")
            };
            return;
        }

        if (context.Type == typeof(CreateTicketCommentDto))
        {
            schema.Example = new OpenApiObject
            {
                ["text"] = new OpenApiString("Ho aggiunto ulteriori dettagli")
            };
            return;
        }

        if (context.Type == typeof(UpdateTicketPriorityDto))
        {
            schema.Example = new OpenApiObject
            {
                ["priority"] = new OpenApiInteger((int)TicketPriority.High)
            };
            return;
        }

        if (context.Type == typeof(UpdateTicketDueDateDto))
        {
            schema.Example = new OpenApiObject
            {
                ["dueAt"] = new OpenApiString("2026-02-01T12:00:00Z")
            };
        }
    }
}
