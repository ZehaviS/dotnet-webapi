using System.Collections.Concurrent;
using System.Diagnostics;

namespace MyMiddleware;

public class MyLogMiddleware
{
    private readonly RequestDelegate next;
    private static readonly ConcurrentQueue<string> logQueue = new();
    private static readonly CancellationTokenSource cts = new();
    private static Task? workerTask;
    private readonly ILogger logger;

    public MyLogMiddleware(RequestDelegate next, ILogger<MyLogMiddleware> logger)
    {
        this.next = next;
        this.logger = logger;
        StartWorker();
    }

    public async Task Invoke(HttpContext c)
    {
        var sw = new Stopwatch();
        sw.Start();
        await next.Invoke(c);
        var log = $"{c.Request.Path}.{c.Request.Method} took {sw.ElapsedMilliseconds}ms. User: {c.User?.FindFirst("Id")?.Value ?? "unknown"}";
        logQueue.Enqueue(log);
    }

    private void StartWorker()
    {
        if (workerTask != null) return;
        workerTask = Task.Run(async () =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                while (logQueue.TryDequeue(out var log))
                {
                    logger.LogDebug(log);
                }
                await Task.Delay(100, cts.Token);
            }
        });
    }
}

public static partial class MiddlewareExtensions
{
    public static IApplicationBuilder UseMyLogMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<MyLogMiddleware>();
    }
}

