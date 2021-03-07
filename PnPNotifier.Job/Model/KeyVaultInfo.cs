namespace PnPNotifier.Job.Model
{
    public class KeyVaultInfo
    {
        public const string SectionName = "KeyVault";

        public string EndpointUrl { get; set; }
        public string CertificateSecretName { get; set; }
    }
}
