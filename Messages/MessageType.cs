namespace SophBot.Messages {
    public class MessageType {
        public static Task Error() {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"[ -  ERROR  - {DateTime.Now} ] ");
            Console.ResetColor();
            
            return Task.CompletedTask;
        }
        public static Task Warning() {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"[ - WARNING - {DateTime.Now} ] ");
            Console.ResetColor();
            
            return Task.CompletedTask;
        }
        public static Task Message() {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($"[ - MESSAGE - {DateTime.Now} ] ");
            Console.ResetColor();
            
            return Task.CompletedTask;
        }
        public static Task Debug() {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($"[ -  DEBUG  - {DateTime.Now} ] ");
            Console.ResetColor();
            
            return Task.CompletedTask;
        }
    }
}