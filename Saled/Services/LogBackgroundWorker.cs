using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Saled.Services
{
    public interface ILogQueue
    {
        void Enqueue(LogMessage message);
        bool TryDequeue(out LogMessage message);
    }

    public class LogQueue : ILogQueue
    {
        private readonly ConcurrentQueue<LogMessage> _queue = new ConcurrentQueue<LogMessage>();
        public void Enqueue(LogMessage message) => _queue.Enqueue(message);
        public bool TryDequeue(out LogMessage message) => _queue.TryDequeue(out message);
    }

    public class LogMessage
    {
        public DateTime StartTime { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Username { get; set; }
        public long DurationMs { get; set; }
    }

    public class LogBackgroundWorker : Microsoft.Extensions.Hosting.BackgroundService
    {
        private readonly ILogQueue _logQueue;
        public LogBackgroundWorker(ILogQueue logQueue)
        {
            _logQueue = logQueue;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logQueue.TryDequeue(out var log))
                {
                    // Append log to file (append mode)
                    var line = $"{log.StartTime:o},{log.Controller},{log.Action},{log.Username},{log.DurationMs}ms";
                    await System.IO.File.AppendAllTextAsync("log.txt", line + Environment.NewLine);
                }
                else
                {
                    await Task.Delay(200, stoppingToken);
                }
            }
        }
    }
}
