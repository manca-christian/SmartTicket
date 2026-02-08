using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Security.Claims;
using SmartTicket.API.Middleware;
using SmartTicket.API.RateLimiting;
using SmartTicket.API.Health;
using SmartTicket.API.Observability;
using SmartTicket.API.Security;
using SmartTicket.API.Services;
using SmartTicket.API.Swagger;
using SmartTicket.Infrastructure.Jobs;
using SmartTicket.Infrastructure.Services;
using SmartTicket.Infrastructure.Observability;
using SmartTicket.Infrastructure.Persistence;
using SmartTicket.Infrastructure.Repositories;
using SmartTicket.Application.Interfaces;
using SmartTicket.Application.Services;
using System.Security.Cryptography;
using System.Text;
using System.Threading.RateLimiting;



Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.Development.json", optional: true)
        .AddEnvironmentVariables()
        .Build())
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) =>
    lc.ReadFrom.Configuration(ctx.Configuration)
);


builder.Services.AddRateLimiter(options =>
{
    var rateOptions = builder.Configuration
        .GetSection("RateLimiting")
        .Get<RateLimitingOptions>() ?? new RateLimitingOptions();

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
        context => RateLimitPolicies.Global(context, rateOptions));

    options.OnRejected = async (context, token) =>
    {
        var httpContext = context.HttpContext;
        var problem = new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Title = "Too Many Requests",
            Type = "https://errors.smartticket.dev/RATE_LIMITED",
            Detail = "Hai superato il limite di richieste.",
            Instance = httpContext.Request.Path
        };

        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        problem.Extensions["correlationId"] = SmartTicket.Application.Observability.CorrelationContext.Current;
        problem.Extensions["errorCode"] = "RATE_LIMITED";

        httpContext.Response.ContentType = "application/problem+json";
        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await httpContext.Response.WriteAsJsonAsync(problem, token);
    };

    options.AddPolicy("auth-login", httpContext => RateLimitPolicies.Login(httpContext, rateOptions));
    options.AddPolicy("auth-refresh", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        httpContext.Request.Cookies.TryGetValue(AuthCookies.RefreshCookieName, out var rt);
        var tokenPart = string.IsNullOrWhiteSpace(rt) ? "no-rt" : Sha256Short(rt);

        var key = $"{ip}|{tokenPart}";

        return RateLimitPartition.GetSlidingWindowLimiter(key, _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = 20,
            Window = TimeSpan.FromMinutes(1),
            SegmentsPerWindow = 6,
            QueueLimit = 0
        });
    });
});

static string Sha256Short(string s)
{
    var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(s));
    return Convert.ToBase64String(bytes.AsSpan(0, 12));
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors(options =>
{
    options.AddPolicy("default", policy =>
    {
        var env = builder.Environment;

        if (env.IsDevelopment())
        {
            policy.WithOrigins("http://localhost:5173", "https://localhost:5173");
        }
        else
        {
            var allowed = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>() ?? Array.Empty<string>();

            policy.WithOrigins(allowed);
        }

        policy.AllowAnyMethod()
            .WithHeaders("Authorization", "Content-Type", "If-Match", "Idempotency-Key", "X-Correlation-Id")
            .AllowCredentials();
    });
});
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerOptions>();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName: "SmartTicket.API"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation();

        if (builder.Environment.IsDevelopment())
        {
            tracing.AddConsoleExporter();
        }

        var otlpEndpoint = builder.Configuration["OpenTelemetry:Otlp:Endpoint"];
        var otlpEnabled = builder.Configuration.GetValue<bool>("OpenTelemetry:Otlp:Enabled");
        if (otlpEnabled && !string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            tracing.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
        }
    })
    .WithMetrics(metrics =>
    {
        metrics
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddRuntimeInstrumentation();

        if (builder.Environment.IsDevelopment())
        {
            metrics.AddConsoleExporter();
        }

        var otlpEndpoint = builder.Configuration["OpenTelemetry:Otlp:Endpoint"];
        var otlpEnabled = builder.Configuration.GetValue<bool>("OpenTelemetry:Otlp:Enabled");
        if (otlpEnabled && !string.IsNullOrWhiteSpace(otlpEndpoint))
        {
            metrics.AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint));
        }
    });


builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default"));
});


builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITicketRepository, TicketRepository>();
builder.Services.AddScoped<ITicketCommentRepository, TicketCommentRepository>();
builder.Services.AddScoped<ITicketEventRepository, TicketEventRepository>();
builder.Services.AddScoped<ITicketAttachmentRepository, TicketAttachmentRepository>();
builder.Services.AddScoped<ICommentAttachmentRepository, CommentAttachmentRepository>();
builder.Services.AddScoped<IdempotencyFilter>();
builder.Services.AddScoped<IIdempotencyKeyRepository, IdempotencyKeyRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
builder.Services.AddScoped<RefreshTokenCleanupRunner>();
builder.Services.AddHostedService<RefreshTokenCleanupJob>();
builder.Services.AddScoped<IAuditWriter, DbAuditWriter>();
builder.Services.AddScoped<LogSecurityAudit>();
builder.Services.AddScoped<DbSecurityAudit>();
builder.Services.AddScoped<ISecurityAudit, CompositeSecurityAudit>();
builder.Services.AddScoped<ITicketAudit, TicketAudit>();
builder.Services.AddScoped<TicketService>();
builder.Services.AddScoped<ITicketCommentService, TicketCommentService>();
builder.Services.AddScoped<ITicketEventService, TicketEventService>();
builder.Services.AddScoped<ITicketEventWriter, TicketEventWriter>();
builder.Services.AddScoped<ITicketService>(sp =>
    new TicketServiceAuditingDecorator(
        sp.GetRequiredService<TicketService>(),
        sp.GetRequiredService<ITicketAudit>(),
        sp.GetRequiredService<ITicketRepository>()
    )
);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    var key = builder.Configuration["Jwt:Key"]!;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],

        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],

        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ClockSkew = TimeSpan.FromSeconds(30),
        NameClaimType = ClaimTypes.NameIdentifier,
        RoleClaimType = ClaimTypes.Role
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("TicketRead", policy => policy.Requirements.Add(new TicketReadRequirement()));
    options.AddPolicy("TicketWrite", policy => policy.Requirements.Add(new TicketWriteRequirement()));
    options.AddPolicy("TicketAssign", policy => policy.Requirements.Add(new TicketAssignRequirement()));
});

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var problem = new ValidationProblemDetails(context.ModelState)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation Error",
            Type = "https://errors.smartticket.dev/VALIDATION_FAILED",
            Instance = context.HttpContext.Request.Path
        };

        problem.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
        problem.Extensions["correlationId"] = SmartTicket.Application.Observability.CorrelationContext.Current;
        problem.Extensions["errorCode"] = "VALIDATION_FAILED";

        return new BadRequestObjectResult(problem)
        {
            ContentTypes = { "application/problem+json" }
        };
    };
});

builder.Services.AddTransient<CorrelationIdHandler>();
builder.Services.AddHealthChecks()
    .AddCheck("live", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: ["live"])
    .AddCheck<DbReadyHealthCheck>("db", tags: ["ready"]);

builder.Services.AddHttpClient("ExternalApi")
    .AddHttpMessageHandler<CorrelationIdHandler>();

builder.Services.Configure<IdempotencyCleanupOptions>(builder.Configuration.GetSection("Idempotency"));
builder.Services.AddHostedService<IdempotencyCleanupJob>();
builder.Services.AddHostedService<OutboxPublisherJob>();

var app = builder.Build();

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.UseMiddleware<ClaimsLoggingMiddleware>();
}
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} => {StatusCode} in {Elapsed:0.0000}ms (Corr:{CorrelationId})";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        var correlationId = SmartTicket.Application.Observability.CorrelationContext.Current
            ?? httpContext.TraceIdentifier;

        diagnosticContext.Set("CorrelationId", correlationId);
        diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);

        if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            try
            {
                diagnosticContext.Set("UserId", httpContext.User.GetUserId());
                diagnosticContext.Set("Role", httpContext.User.IsAdmin() ? "Admin" : "User");
            }
            catch
            {
            }
        }
    };
});

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
            $"SmartTicket API {description.GroupName.ToUpperInvariant()}");
    }
});

app.UseCors("default");

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("live"),
    ResponseWriter = HealthResponseWriter.WriteAsync,
    ResultStatusCodes =
    {
        [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy] = StatusCodes.Status200OK,
        [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded] = StatusCodes.Status200OK,
        [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready"),
    ResponseWriter = HealthResponseWriter.WriteAsync,
    ResultStatusCodes =
    {
        [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy] = StatusCodes.Status200OK,
        [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Degraded] = StatusCodes.Status200OK,
        [Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    }
});

app.UseRateLimiter();

app.MapControllers();

app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);

app.Run();
