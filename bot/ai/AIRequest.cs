using System.Security.Cryptography.X509Certificates;

namespace SophBot.bot.ai
{
    public class AIRequest
    {
        public string User { get; private set; }
        public string Promt { get; private set; }
        public AIResponseChannelType ChannelType { get; private set; }
        public int CharLimit { get; private set; }
        public AIRequest(string user, string promt, AIResponseChannelType type, int? charLimit = null)
        {
            User = user;
            Promt = promt;
            ChannelType = type;

            if (charLimit is not null)
            {
                CharLimit = (int)charLimit;
            }
            else
            {
                CharLimit = type switch
                {
                    AIResponseChannelType.Discord => 2000,
                    AIResponseChannelType.Twitch => 500,
                    _ => 500
                };
            }
        }

#pragma warning disable CS0114 
        public string ToString()
        {
            return $"{User} schreibt auf {ChannelType}: \"{Promt}\". Beachte das Zeichenlimit von max. {CharLimit} Zeichen, sowie ggf. andere Systemanweisungen die du bei {ChannelType} beachten musst!";
        }
#pragma warning restore CS0114 
    }
    public enum AIResponseChannelType
    {
        Discord,
        Twitch
    }
}