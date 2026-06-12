using Microsoft.Extensions.Logging;

namespace SophBot.Universal
{
    public class Logger : ILogger
    {
        string _source;
        DatabaseService _dbSercive;
        public Logger(string source, DatabaseService dbService) 
        {
            _source = source;
            _dbSercive = dbService;
        }

#pragma warning disable CS8633 // Nullability in constraints for type parameter doesn't match the constraints for type parameter in implicitly implemented interface method'.
        public IDisposable? BeginScope<TState>(TState state) => null;
#pragma warning restore CS8633 // Nullability in constraints for type parameter doesn't match the constraints for type parameter in implicitly implemented interface method'.

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _dbSercive.InsertData(DBTable.Logs, new Dictionary<string, object>
            {
                {"source", _source},
                {"datetime", DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss:fff")},
                {"loglevel", logLevel.ToString()},
                {"content", formatter(state, exception)}
            });
        }
    }
    public class LoggingProvider : ILoggerProvider
    {
        DatabaseService _dbService;
        public LoggingProvider(DatabaseService dbService)
        {
            _dbService = dbService;
            _dbService.StartAsync(CancellationToken.None).Wait();
        }
        public ILogger CreateLogger(string categoryName)
        {
            return new Logger(categoryName, _dbService);
        }

        public void Dispose()
        {
        }
    }
}