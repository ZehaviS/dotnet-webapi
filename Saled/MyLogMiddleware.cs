using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Saled.Services;

namespace MyMiddleware;

public class MyLogMiddleware
{
    private readonly RequestDelegate next;
    private readonly ILogQueue logQueue;

    public MyLogMiddleware(RequestDelegate next, ILogQueue logQueue)
    {
        this.next = next;
        this.logQueue = logQueue;
    }

    public async Task Invoke(HttpContext c)
    {
        var sw = new Stopwatch();
        sw.Start();
        await next.Invoke(c);
        sw.Stop();
        var log = new LogMessage
        {
            StartTime = DateTime.UtcNow,
            Controller = c.Request.Path,
            Action = c.Request.Method,
            Username = c.User?.FindFirst("Id")?.Value ?? "unknown",
            DurationMs = sw.ElapsedMilliseconds
        };
        logQueue.Enqueue(log);
    }
}

public static partial class MiddlewareExtensions
{
    public static IApplicationBuilder UseMyLogMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<MyLogMiddleware>();
    }
}

