namespace SophBot.bot.database
{
    public enum SDBColumn
    {
        UserID,
        Points,
        ServerID,
        RuleChannelID,
        WelcomeChannelID,
        NotificationChannelID,
        TwitchChannel,
        MemberRoleID,
        MentionRoleID,
        Name,
        Description,
        Number,
        DateTime
    }

    public class SDBValue
    {
        public SDBColumn Column;
        public string Value;

        public SDBValue(SDBColumn column, string value)
        {
            this.Value = value;
            this.Column = column;
        }
    }
}