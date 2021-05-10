using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MicroCommerce.Middleware
{
    public static class RequestLoggingExt
    {
        private static RequestLoggingOptions Options = new RequestLoggingOptions();

        public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder, params string[] exclude)
        {
            Options.Exclude = exclude;

            return builder.UseMiddleware<RequestLoggingMiddleware>();
        }

        public static IServiceCollection AddRequestLogging(this IServiceCollection services)
        {
            return services.AddSingleton(Options);
        }
    }

    internal class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private RequestLoggingOptions _options;

        public RequestLoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, RequestLoggingOptions options)
        {
            _next = next;
            _options = options;
            _logger = loggerFactory.CreateLogger("LoggingMiddleware");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (_options.Exclude.Any(i => context.Request.Path.Value.Trim().ToLower().Contains(i)))
            {
                await _next.Invoke(context);
                return;
            }

            var request = context.Request;
            _logger.LogInformation($"Incoming request: {request.Method}, {request.Path}, [{HeadersToString(request.Headers)}]");
            await _next.Invoke(context);
            var response = context.Response;
            _logger.LogInformation($"Outgoing response: {response.StatusCode}, [{HeadersToString(response.Headers)}]");
        }

        private string HeadersToString(IHeaderDictionary headers)
        {
            var list = new List<string>();
            foreach (var key in headers.Keys)
            {
                list.Add($"'{key}':[{string.Join(';', headers[key])}]");
            }

            return string.Join(", ", list);
        }
    }

    internal class RequestLoggingOptions
    {
        public string[] Exclude = new string[] { };
    }
}