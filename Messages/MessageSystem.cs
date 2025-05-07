namespace SophBot.Messages {
    public class MessageSystem {
        public static async Task sendMessage (string message, Task MessageType) {
            Console.WriteLine(message);
            await Task.Delay(1);
        }
    }
}