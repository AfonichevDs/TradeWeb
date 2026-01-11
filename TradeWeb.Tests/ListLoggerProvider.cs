using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace TradeWeb.Tests;
internal class ListLoggerProvider : ILoggerProvider
{
    public ConcurrentQueue<(LogLevel Level, string Message)> Entries { get; } = new();

    public ILogger CreateLogger(string categoryName) => new ListLogger(Entries);
    public void Dispose() { }

    private sealed class ListLogger : ILogger
    {
        private readonly ConcurrentQueue<(LogLevel, string)> _entries;
        public ListLogger(ConcurrentQueue<(LogLevel, string)> entries) => _entries = entries;

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId,
            TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _entries.Enqueue((logLevel, formatter(state, exception)));
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}
