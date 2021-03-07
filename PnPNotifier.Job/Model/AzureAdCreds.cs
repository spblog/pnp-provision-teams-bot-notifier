namespace PnPNotifier.Job.Model
{
    public class AzureAdCreds
    {
        public const string SectionName = "AzureAdApp";

        public string ClientId { get; set; }
        public string TenantId { get; set; }
        public string PfxPath { get; set; }
        public string PfxPassword { get; set; }
    }
}
