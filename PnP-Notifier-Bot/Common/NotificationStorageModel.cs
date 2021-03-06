using System.Collections.Generic;

namespace PnPNotifierBot.Common
{
    public class NotificationStorageModel
    {
        public List<NotificationData> NotificationsData { get; set; } = new List<NotificationData>();
    }
}
