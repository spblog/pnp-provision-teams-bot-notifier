using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SharePoint.Client;
using PnP.Framework;
using PnP.Framework.Provisioning.Connectors;
using PnP.Framework.Provisioning.ObjectHandlers;
using PnP.Framework.Provisioning.Providers.Xml;
using PnPNotifier.Job.Model;
using PnPNotifier.Job.Notifications;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PnPNotifier.Job
{
    public class Functions
    {
        private readonly AzureAdCreds _azureCreds;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly NotificationCardManager _notificationCardManager;
        private readonly string _templateName = "ContosoLanding.pnp";

        public Functions(
            IOptions<AzureAdCreds> azureOpts,
            IHostingEnvironment hostingEnvironment,
            NotificationCardManager notificationCardManager)
        {
            _azureCreds = azureOpts.Value;
            _hostingEnvironment = hostingEnvironment;
            _notificationCardManager = notificationCardManager;
        }

        public async Task ProcessQueueMessage([QueueTrigger("pnp-provision")] Model.Site siteModel, ILogger logger)
        {
            try
            {
                logger.LogInformation($"Starting processing web {siteModel.WebUrl}");

                var authManager = CreateAuthManagerWithLocalCertificate();

                var clientContext = await authManager.GetContextAsync(siteModel.WebUrl);

                var web = clientContext.Web;
                clientContext.Load(web);
                await clientContext.ExecuteQueryRetryAsync();

                await _notificationCardManager.SendStartingCardAsync(web.Url, web.Title, _templateName);
                var applyInfo = _notificationCardManager.CreateApplyingInfo();

                Provision(web, applyInfo);

                await _notificationCardManager.SendSuccessCardAsync(web.Url);

                logger.LogInformation("Finished");
            }
            catch (Exception ex)
            {
                logger.LogError(new EventId(), ex, ex.Message);
                await _notificationCardManager.SendErrorCardAsync(ex);
                throw;
            }
        }

        private void Provision(Web web, ProvisioningTemplateApplyingInformation applyInfo)
        {
            var fileConnector = new FileSystemConnector(_hostingEnvironment.ContentRootPath, string.Empty);
            var provider = new XMLOpenXMLTemplateProvider(_templateName, fileConnector);

            var template = provider.GetTemplates().ToList().First();
            template.Connector = provider.Connector;

            web.ApplyProvisioningTemplate(template, applyInfo);
        }

        private AuthenticationManager CreateAuthManagerWithLocalCertificate()
        {
            return new AuthenticationManager(_azureCreds.ClientId, _azureCreds.PfxPath, _azureCreds.PfxPassword, _azureCreds.TenantId);
        }
    }
}
