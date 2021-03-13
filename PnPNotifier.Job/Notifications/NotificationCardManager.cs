using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PnP.Framework.Provisioning.ObjectHandlers;
using PnPNotifier.Common.Config;
using PnPNotifier.Common.Notifications;
using PnPNotifier.Job.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Activity = Microsoft.Bot.Schema.Activity;

namespace PnPNotifier.Job.Notifications
{
    public class NotificationCardManager
    {
        private readonly BotCredentials _botCredentials;
        private List<NotificationData> _notificationData;
        private readonly Dictionary<string, string> _conversationMap;
        private readonly Dictionary<string, string> _activityMap;
        private readonly AzureAdCreds _azureCreds;
        private readonly NotificationsManager _notificationsManager;
        private Stopwatch _stopwatch;
        private readonly IHostingEnvironment _hostingEnvironment;

        public NotificationCardManager(
            IOptions<BotCredentials> botOptions, 
            NotificationsManager notificationsManager,
            IHostingEnvironment hostingEnvironment,
            IOptions<AzureAdCreds> azureOpts)
        {
            _botCredentials = botOptions.Value;
            _conversationMap = new Dictionary<string, string>();
            _activityMap = new Dictionary<string, string>();
            _azureCreds = azureOpts.Value;
            _notificationsManager = notificationsManager;
            _stopwatch = new Stopwatch();
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task SendStartingCardAsync(string siteUrl, string siteName, string templateName)
        {
            _stopwatch.Start();
            _notificationData = await _notificationsManager.GetAllNotifcationsAsync();

            var thumbnailCard = new ThumbnailCard
            {
                Title = "Starting a new PnP provisioning process",
                Subtitle = "See the details and progress below",
                Text = $"<strong>Site:</strong> <a href=\"{siteUrl}\" target=\"_blank\">{siteName}</a><br>" +
                $"<strong>Template:</strong> {Path.GetFileNameWithoutExtension(templateName)}",
                Images = new List<CardImage>
                {
                    new CardImage(CreateBase64FromImage("start.png"))
                }
            };

            foreach (var data in _notificationData)
            {
                var client = new ConnectorClient(new Uri(data.ServiceUrl), GetMicrosoftAppCredentials(), new HttpClient());
                var conversationParams = new ConversationParameters
                {
                    IsGroup = true,
                    ChannelData = new TeamsChannelData
                    {
                        Channel = new ChannelInfo(data.ChannelId),
                    },
                    Bot = new ChannelAccount
                    {
                        Id = _botCredentials.MicrosoftAppId
                    },
                    TenantId = _azureCreds.TenantId,
                    Activity = (Activity)MessageFactory.Attachment(thumbnailCard.ToAttachment())
                };
                var conversation = await client.Conversations.CreateConversationAsync(conversationParams);
                _conversationMap.Add(data.ChannelId, conversation.Id);
            }
        }

        public async Task SendSuccessCardAsync(string siteUrl)
        {
            _notificationData = await _notificationsManager.GetAllNotifcationsAsync();
            _stopwatch.Stop();

            var thumbnailCard = new ThumbnailCard
            {
                Title = "PnP provisioning is successfully completed",
                Subtitle = $"Time elapsed: {string.Format("{0} min {1} sec", _stopwatch.Elapsed.Minutes, _stopwatch.Elapsed.Seconds)}",
                Images = new List<CardImage>
                {
                    new CardImage(CreateBase64FromImage("success.png"))
                },
                Buttons = new List<CardAction>
                {
                    new CardAction
                    {
                        Type = ActionTypes.OpenUrl,
                        Title = "Open site",
                        Value = siteUrl
                    }
                }
            };

            foreach (var data in _notificationData)
            {
                var client = new ConnectorClient(new Uri(data.ServiceUrl), GetMicrosoftAppCredentials(), new HttpClient());
                var conversationParams = new ConversationParameters
                {
                    IsGroup = true,
                    ChannelData = new TeamsChannelData
                    {
                        Channel = new ChannelInfo(data.ChannelId),
                    },
                    Bot = new ChannelAccount
                    {
                        Id = _botCredentials.MicrosoftAppId
                    },
                    TenantId = _azureCreds.TenantId,
                    Activity = (Activity)MessageFactory.Attachment(thumbnailCard.ToAttachment())
                };
                var conversationId = _conversationMap[data.ChannelId];
                var replyToId = _activityMap[data.ChannelId];
                await client.Conversations.UpdateActivityAsync(conversationId, replyToId, conversationParams.Activity);
            }
        }

        public async Task SendErrorCardAsync(Exception ex)
        {
            try {
                _notificationData = await _notificationsManager.GetAllNotifcationsAsync();
                _stopwatch.Stop();

                var thumbnailCard = new ThumbnailCard
                {
                    Title = "An error occured",
                    Text = $"<strong>Message:</strong> {ex.Message}<br>" +
                    $"<strong>Stack trace:</strong><pre><code>{ex.StackTrace}</code></pre>",
                    Images = new List<CardImage>
                {
                    new CardImage(CreateBase64FromImage("error.png"))
                },
                    Buttons = new List<CardAction>
                {
                    new CardAction
                    {
                        Type = ActionTypes.OpenUrl,
                        Title = "See detailed logs",
                        Value = "http://example.com"
                    }
                }
                };

                foreach (var data in _notificationData)
                {
                    var client = new ConnectorClient(new Uri(data.ServiceUrl), GetMicrosoftAppCredentials(), new HttpClient());
                    var conversationParams = new ConversationParameters
                    {
                        IsGroup = true,
                        ChannelData = new TeamsChannelData
                        {
                            Channel = new ChannelInfo(data.ChannelId),
                        },
                        Bot = new ChannelAccount
                        {
                            Id = _botCredentials.MicrosoftAppId
                        },
                        TenantId = _azureCreds.TenantId,
                        Activity = (Activity)MessageFactory.Attachment(thumbnailCard.ToAttachment())
                    };
                    var conversationId = _conversationMap[data.ChannelId];
                    var replyToId = _activityMap[data.ChannelId];
                    await client.Conversations.UpdateActivityAsync(conversationId, replyToId, conversationParams.Activity);
                }
            }
            catch (Exception)
            {
                // if exception is thrown at the earlier stage, we cannot send card, thus just ignore possible exceptions
            }
        }

        public ProvisioningTemplateApplyingInformation CreateApplyingInfo()
        {
            return new ProvisioningTemplateApplyingInformation
            {
                ProgressDelegate = (message, progress, total) =>
                {
                    Task.Run(async () => await SendUpdate(message, progress, total)).Wait();
                }
            };
        }

        private string CreateBase64FromImage(string name)
        {
            var imgPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Resources", name);
            using (var image = Image.FromFile(imgPath))
            {
                using (var m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    var imageBytes = m.ToArray();

                    return "data:image/png;base64," + Convert.ToBase64String(imageBytes);
                }
            }
        }

        private async Task SendUpdate(string message, int progress, int total)
        {
            var thumbnailCard = new ThumbnailCard
            {
                Title = "The provisioning is in progress",
                Text = $"<strong>Processing object:</strong> {message}<br>" +
                $"<strong>Step:</strong> {progress} out of {total}"
            };

            foreach (var notificationData in _notificationData)
            {
                var client = new ConnectorClient(new Uri(notificationData.ServiceUrl), GetMicrosoftAppCredentials(), new HttpClient());
                var conversationParams = new ConversationParameters
                {
                    IsGroup = true,
                    ChannelData = new TeamsChannelData
                    {
                        Channel = new ChannelInfo(notificationData.ChannelId),
                    },
                    Bot = new ChannelAccount
                    {
                        Id = _botCredentials.MicrosoftAppId
                    },
                    TenantId = _azureCreds.TenantId,
                    Activity = (Activity)MessageFactory.Attachment(thumbnailCard.ToAttachment())
                };

                var conversationId = _conversationMap[notificationData.ChannelId];

                if (_activityMap.ContainsKey(notificationData.ChannelId))
                {
                    var replyToId = _activityMap[notificationData.ChannelId];
                    await client.Conversations.UpdateActivityAsync(conversationId, replyToId, conversationParams.Activity);
                }
                else
                {
                    var result = await client.Conversations.SendToConversationAsync(conversationId, conversationParams.Activity);
                    _activityMap.Add(notificationData.ChannelId, result.Id);
                }
            }
        }

        private MicrosoftAppCredentials GetMicrosoftAppCredentials()
        {
            return new MicrosoftAppCredentials(_botCredentials.MicrosoftAppId,
                _botCredentials.MicrosoftAppPassword);
        }
    }
}
