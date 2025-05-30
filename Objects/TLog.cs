namespace SophBot.Objects
{
    public static class TLog
    {
        public enum MessageType
        {
            Message,
            Warning,
            Error,
            Debug
        }

        public static void sendLog(string message, MessageType type = MessageType.Debug)
        {
            if (type == MessageType.Message)
            {
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write($"[ - MESSAGE - {DateTime.Now} ] ");
                Console.ResetColor();
            }
            else if (type == MessageType.Warning)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"[ - WARNING - {DateTime.Now} ] ");
                Console.ResetColor();
            }
            else if (type == MessageType.Error)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"[ -  ERROR  - {DateTime.Now} ] ");
                Console.ResetColor();
            }
            else if (type == MessageType.Debug)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write($"[ -  DEBUG  - {DateTime.Now} ] ");
                Console.ResetColor();
            }

            Console.WriteLine(message);
        }
    }
}