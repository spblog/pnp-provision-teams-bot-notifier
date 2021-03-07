using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Teams;
using Microsoft.Bot.Schema;
using PnPNotifier.Common.Notifications;
using PnPNotifier.Bot.Cards.Managers;
using System.Threading;
using System.Threading.Tasks;

namespace PnPNotifier.Bot.Cards.Handlers
{
    public class ConfigurePnPNotificationsCardHandler : CardHandler
    {
        private readonly NotificationsManager _notificationsManager;
        private readonly ConfigureNotificationsCardManager _cardManager;

        public ConfigurePnPNotificationsCardHandler(NotificationsManager notificationsManager, ConfigureNotificationsCardManager cardManager)
        {
            _notificationsManager = notificationsManager;
            _cardManager = cardManager;
        }

        public override async Task ExecuteAsync(CardPayload payload, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            switch (payload.CardActionType)
            {
                case CardActionType.EnableNotifications:
                    {
                        await OnEnableNotifications(turnContext, cancellationToken);
                        break;
                    }
                case CardActionType.DisableNotifications:
                    {
                        await OnDisableNotifications(turnContext, cancellationToken);
                        break;
                    }
            }
        }

        private async Task OnEnableNotifications(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var channelId = turnContext.Activity.TeamsGetChannelId();
            var notificationsData = new NotificationData
            {
                ChannelId = channelId,
                ServiceUrl = turnContext.Activity.ServiceUrl,
                TenantId = turnContext.Activity.Conversation.TenantId
            };

            await _notificationsManager.EnableNotificationsAsync(notificationsData);

            var card = _cardManager.CreateCard(true);

            var updatedActivity = MessageFactory.Attachment(card.ToAttachment());
            updatedActivity.Id = turnContext.Activity.ReplyToId;
            await turnContext.UpdateActivityAsync(updatedActivity, cancellationToken);
        }

        private async Task OnDisableNotifications(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var channelId = turnContext.Activity.TeamsGetChannelId();

            await _notificationsManager.DisableNotificationsAsync(channelId);

            var card = _cardManager.CreateCard(false);

            var updatedActivity = MessageFactory.Attachment(card.ToAttachment());
            updatedActivity.Id = turnContext.Activity.ReplyToId;
            await turnContext.UpdateActivityAsync(updatedActivity, cancellationToken);
        }
    }
}
