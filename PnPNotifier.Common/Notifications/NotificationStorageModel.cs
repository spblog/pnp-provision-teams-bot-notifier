using System.Collections.Generic;

namespace PnPNotifier.Common.Notifications
{
    public class NotificationStorageModel
    {
        public List<NotificationData> NotificationsData { get; set; } = new List<NotificationData>();
    }
}
