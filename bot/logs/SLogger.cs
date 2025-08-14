using DSharpPlus;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using SophBot.bot.conf;
using System;
namespace SophBot.bot.logs
{
    public static class SLogger
    {
        public static void Log(LogLevel level, string message, string source, Exception exception = null!)
        {
            if (!File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}/logs/bot.txt"))
                File.Create($"{AppDomain.CurrentDomain.BaseDirectory}/logs/bot.txt");
            if (level < SConfig.LogLevel) return;

            var time = DateTime.Now.ToString("dd.MM.yyyy HH:mm");

            Console.ForegroundColor = GetColor(level);
            Console.Write($"[{time} - {level}] ");
            Console.ResetColor();
            Console.WriteLine($"{source} -> {message}");

            if (exception != null)
                Console.WriteLine(exception);


            File.AppendAllText($"{AppDomain.CurrentDomain.BaseDirectory}/logs/bot.txt", $"[{time} - {level}] {source} -> {message}\n{((exception != null) ? exception + "\n" : "")}");
        }
        public static void LogAi(string user, string request, string response, bool isFailed, Exception? exception = null)
        {
            if (!File.Exists($"{AppDomain.CurrentDomain.BaseDirectory}/logs/ai.txt"))
                File.Create($"{AppDomain.CurrentDomain.BaseDirectory}/logs/ai.txt");

            string logMessage = @$"
-- MSG {(isFailed ? "FAILED" : "SENDET")} --
[{user} at {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}] => {request}

[SophBot AI at {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}] => {response}
--  END MSG  --
";
            if (exception != null) logMessage += $"Exception of msg above: {exception}\n";
            File.AppendAllText($"{AppDomain.CurrentDomain.BaseDirectory}/logs/ai.txt", logMessage);
        }

        private static ConsoleColor GetColor(LogLevel level) => level switch
        {
            LogLevel.Critical => ConsoleColor.DarkRed,
            LogLevel.Error => ConsoleColor.Red,
            LogLevel.Warning => ConsoleColor.Yellow,
            LogLevel.Information => ConsoleColor.Blue,
            LogLevel.Debug => ConsoleColor.DarkGray,
            LogLevel.Trace => ConsoleColor.Black,
            LogLevel.None => ConsoleColor.Green,
            _ => ConsoleColor.White,
        };


        public class LoggerProvider : ILoggerProvider
        {
            #pragma warning disable CS8603, CS8633, CS8767
            public ILogger CreateLogger(string categoryName)
            {
                return new StaticLogger(categoryName);
            }

            public void Dispose() { }

            private class StaticLogger : ILogger
            {
                private readonly string _name;

                public StaticLogger(string name)
                {
                    _name = name;
                }

                public IDisposable BeginScope<TState>(TState state) => null;

                public bool IsEnabled(LogLevel logLevel) => true;

                public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
                {
                    if (SConfig.LogLevel >= LogLevel.Information && logLevel <= LogLevel.Information) return;
                    SLogger.Log(logLevel, formatter(state, exception), _name, exception);
                }
            }
        }
    }
}