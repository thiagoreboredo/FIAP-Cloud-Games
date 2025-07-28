using Application.Helper;
using Infrastructure.Middleware;
using Microsoft.Extensions.Primitives;

namespace FIAP_Cloud_Games.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class CorrelationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAppLogger<CorrelationMiddleware> _logger;
        private const string _correlationIdHeader = "x-correlation-id";

        public CorrelationMiddleware(RequestDelegate next, IAppLogger<CorrelationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public Task Invoke(HttpContext httpContext, ICorrelationIdGenerator correlationIdGenerator)
        {
            var correlationId = GetCorrelationId(httpContext, correlationIdGenerator);
            AddCorrelationIdHeaderToResponse(httpContext, correlationId);

            _logger.LogInformation($"Request: {httpContext.Request.Method} {httpContext.Request.Path}");

            return _next(httpContext);
        }

        private static StringValues GetCorrelationId(HttpContext httpContext, ICorrelationIdGenerator correlationIdGenerator)
        {
            if (httpContext.Request.Headers.TryGetValue(_correlationIdHeader, out var correlationId))
            {
                correlationIdGenerator.Set(correlationId);
                return correlationId;
            }
            else
            {
                correlationId = Guid.NewGuid().ToString();
                correlationIdGenerator.Set(correlationId);
                return correlationId;
            }
        }

        private static void AddCorrelationIdHeaderToResponse(HttpContext context, StringValues correlationId)
       => context.Response.OnStarting(() =>
       {
           context.Response.Headers[_correlationIdHeader] = new[] { correlationId.ToString() };
           return Task.CompletedTask;
       });
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class CorrelationMiddlewareExtensions
    {
        public static IApplicationBuilder UseCorrelationMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CorrelationMiddleware>();
        }
    }
}
