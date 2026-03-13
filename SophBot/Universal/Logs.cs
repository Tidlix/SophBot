using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;

namespace SophBot.Universal
{
    public static class Logs
    {
        public static void AddLog(string content, LogLevel logLevel = LogLevel.Information, string source = "SophBot") 
        {
            string currentTime = DateTime.Now.ToString("yyyy/MM/dd - HH:mm:ss:fff"); 
            DatabaseEngine.InsertData(DatabaseEngine.DBTable.Logs, new Dictionary<string, object>
            {
                {"source", source},
                {"datetime", currentTime},
                {"loglevel", logLevel.ToString()},
                {"content", content}
            });
        }

        public class LogProvider : ILoggerProvider
        {
            public ILogger CreateLogger(string categoryName)
            {
                return new Logger(categoryName);
            }

            public void Dispose() {}

            private class Logger : ILogger
            {
                private string _name;
                public Logger(string name) {
                    _name = name;
                }
#pragma warning disable CS8633 
                public IDisposable? BeginScope<TState>(TState state) => null;
#pragma warning restore CS8633 
                public bool IsEnabled(LogLevel logLevel) => true;

                public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
                {
                    AddLog(formatter(state, exception), logLevel, _name);
                }
            }
        }
    }
}