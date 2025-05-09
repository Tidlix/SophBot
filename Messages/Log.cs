namespace SophBot.Messages {
    public class Log {
        public static async Task sendMessage (string message, Task? MessageType = null) {
            if (MessageType == null) Console.Write($"[ -  DEBUG  - {DateTime.Now} ] ");
            Console.WriteLine(message);
            await Task.Delay(1);
        }
    }
}