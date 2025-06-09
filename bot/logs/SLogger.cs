using SophBot.bot.conf;

namespace SophBot.bot.logs
{
    public enum LogType
    {
        Debug,
        Message,
        Warning,
        Error,
        Critical
    }

    public class SLogger
    {
        public static void Log(string message, string location = "", Exception? exception = null, LogType type = LogType.Debug)
        {
            if (type < SConfig.LogLevel) return;

            string dateTime = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
            switch (type)
            {
                case LogType.Debug:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($"[ Debug    - {dateTime} ] ");
                    break;
                case LogType.Message:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.Write($"[ Message  - {dateTime} ] ");
                    break;
                case LogType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($"[ Warning  - {dateTime} ] ");
                    break;
                case LogType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"[ Error    - {dateTime} ] ");
                    break;
                case LogType.Critical:
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.Write($"[ Critical - {dateTime} ] ");
                    break;
            }
            if (location != "") Console.Write($"at {location}: ");
            Console.ResetColor();

            Console.WriteLine(message);

            if (exception != null)
            {
                Console.WriteLine($"Given Exeption: {exception.Message}");
            }
        }
    }
}