using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.WebJobs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SharePoint.Client;
using PnP.Framework;
using PnP.Framework.Provisioning.Connectors;
using PnP.Framework.Provisioning.ObjectHandlers;
using PnP.Framework.Provisioning.Providers.Xml;
using PnPNotifier.Common.Config;
using PnPNotifier.Common.Notifications;
using PnPNotifier.Job.Model;
using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace PnPNotifier.Job
{
    public class Functions
    {
        private readonly KeyVaultInfo _keyVaultInfo;
        private readonly AzureAdCreds _azureCreds;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly NotificationsManager _notificationsManager;
        private readonly BotCredentials _botCredentials;

        public Functions(
            IOptions<KeyVaultInfo> keyVaultOpts,
            IOptions<AzureAdCreds> azureOpts,
            IHostingEnvironment hostingEnvironment, 
            NotificationsManager notificationsManager,
            IOptions<BotCredentials> botOptions)
        {
            _keyVaultInfo = keyVaultOpts.Value;
            _azureCreds = azureOpts.Value;
            _hostingEnvironment = hostingEnvironment;
            _notificationsManager = notificationsManager;
            _botCredentials = botOptions.Value;
        }

        public async Task ProcessQueueMessage([QueueTrigger("pnp-drone")] Model.Site siteModel, ILogger logger)
        {
            try
            {
                logger.LogInformation($"Starting processing web {siteModel.WebUrl}");

                // if you have local certificate use this line
                var authManager = CreateAuthManagerWithLocalCertificate();

                // if you use Azure Key Vault, use this line
                //var authManager = await CreateAuthManagerWithKeyVault();
                var hasChannel = await _notificationsManager.IsNotificationsEnabledAsync("ff");
                var clientContext = await authManager.GetContextAsync(siteModel.WebUrl);

                var web = clientContext.Web;
                clientContext.Load(web);
                await clientContext.ExecuteQueryRetryAsync();

                

                logger.LogInformation("Finished");
            }
            catch (Exception ex)
            {
                logger.LogError(new EventId(), ex, ex.Message);
                throw;
            }
        }

        private async Task ProvisionAsync(Web web, ILogger logger)
        {
            var notificationsSettings = await _notificationsManager.GetAllNotifcationsAsync();
            var applyingInformation = new ProvisioningTemplateApplyingInformation
            {
                ProgressDelegate = (message, progress, total) =>
                {
                    foreach (var setting in notificationsSettings)
                    {
                        Task.Run(async () => await WriteTeams(setting)).Wait();
                    }
                    logger.LogInformation("{0:00}/{1:00} - {2}", progress, total, message);
                }
            };

            var fileConnector = new FileSystemConnector(_hostingEnvironment.ContentRootPath, string.Empty);
            var provider = new XMLOpenXMLTemplateProvider("ContosoDroneLanding.pnp", fileConnector);

            var template = provider.GetTemplates().ToList().First();
            template.Connector = provider.Connector;
            web.ApplyProvisioningTemplate(template, applyingInformation);
        }

        private async Task WriteTeams(NotificationData notificationData)
        {
            AppCredentials.TrustServiceUrl(notificationData.ServiceUrl);

            var client = new ConnectorClient(new Uri(notificationData.ServiceUrl), GetMicrosoftAppCredentials(),
                    new HttpClient());


        }

        private async Task<AuthenticationManager> CreateAuthManagerWithKeyVaultAsync()
        {
            var cert = await GetCertificate(_keyVaultInfo.EndpointUrl, _keyVaultInfo.CertificateSecretName);
            return new AuthenticationManager(_azureCreds.ClientId, cert, _azureCreds.TenantId);
        }

        private AuthenticationManager CreateAuthManagerWithLocalCertificate()
        {
            return new AuthenticationManager(_azureCreds.ClientId, _azureCreds.PfxPath, _azureCreds.PfxPassword, _azureCreds.TenantId);
        }

        private MicrosoftAppCredentials GetMicrosoftAppCredentials()
        {
            return new MicrosoftAppCredentials(_botCredentials.MicrosoftAppId,
                _botCredentials.MicrosoftAppPassword);
        }

        private async Task<X509Certificate2> GetCertificate(string keyVaultUri, string kvCertificateSecretName)
        {
            var secretClient = new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential());
            var secretResponse = await secretClient.GetSecretAsync(kvCertificateSecretName);
            var secretData = secretResponse.Value;
            return new X509Certificate2(Convert.FromBase64String(secretData.Value));
        }
    }
}
