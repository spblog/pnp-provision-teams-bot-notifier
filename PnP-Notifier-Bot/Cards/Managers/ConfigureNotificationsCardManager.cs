using Microsoft.Bot.Schema;
using PnPNotifierBot.Common;
using System.Collections.Generic;

namespace PnPNotifierBot.Cards.Managers
{
    public class ConfigureNotificationsCardManager
    {
        private const string _title = "Configure PnP notifications";
        private const string _description = "When enabled, PnP provisioning engine will send site provisioning progress notifications to this channel";
        private const string _subTitleEnabled = "Current state: notifications are enabled";
        private const string _subTitleDisabled = "Current state: notifications are disabled";
        private const string _buttonEnable = "Enable notifications";
        private const string _buttonDisable = "Disable notifications";

        public ThumbnailCard CreateCard(bool notificationsEnabled)
        {
            var thumbnailCard = new ThumbnailCard
            {
                Title = _title,
                Text = _description,
                Subtitle = notificationsEnabled ? _subTitleEnabled : _subTitleDisabled,
                Buttons = new List<CardAction>()
            };

            var enableAction = new CardAction
            {
                Type = ActionTypes.MessageBack,
                Value = new CardPayload { CardActionType = CardActionType.EnableNotifications, CardType = CardType.ConfigurePnPNotifications },
                Title = _buttonEnable,
            };
            var disableAction = new CardAction
            {
                Type = ActionTypes.MessageBack,
                Value = new CardPayload { CardActionType = CardActionType.DisableNotifications, CardType = CardType.ConfigurePnPNotifications },
                Title = _buttonDisable
            };

            if (!notificationsEnabled)
            {
                thumbnailCard.Buttons.Add(enableAction);
            }
            else
            {
                thumbnailCard.Buttons.Add(disableAction);
            }

            return thumbnailCard;
        }
    }
}
