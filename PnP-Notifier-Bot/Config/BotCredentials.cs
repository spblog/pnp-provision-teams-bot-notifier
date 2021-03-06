namespace PnPNotifierBot.Config
{
    public class BotCredentials
    {
        public const string SectionName = "BotCredentials";

        public string MicrosoftAppId { get; set; }
        public string MicrosoftAppPassword { get; set; }
    }
}
