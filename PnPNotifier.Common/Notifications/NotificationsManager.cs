using Microsoft.Bot.Builder;
using PnPNotifierBot.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnPNotifier.Common.Notifications
{
    public class NotificationsManager
    {
        private readonly IStorage _storage;

        public NotificationsManager(IStorage storage)
        {
            _storage = storage;
        }

        public async Task<bool> IsNotificationsEnabledAsync(string channelId)
        {
            var notifications = await _storage.ReadAsync<NotificationStorageModel>(Consts.StorageNotificationsKey);
            if (notifications == null)
            {
                return false;
            }

            return notifications.NotificationsData.Any(n => n.ChannelId.Equals(channelId, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<List<NotificationData>> GetAllNotifcationsAsync()
        {
            var notifications = await _storage.ReadAsync<NotificationStorageModel>(Consts.StorageNotificationsKey);
            if (notifications == null)
            {
                return new List<NotificationData>();
            }

            return notifications.NotificationsData;
        }

        public async Task EnableNotificationsAsync(NotificationData data)
        {
            if(await IsNotificationsEnabledAsync(data.ChannelId))
            {
                return;
            }

            var notifications = await _storage.ReadAsync<NotificationStorageModel>(Consts.StorageNotificationsKey);
            if (notifications == null)
            {
                notifications = new NotificationStorageModel();
            }

            notifications.NotificationsData.Add(data);
            await _storage.WriteAsync(Consts.StorageNotificationsKey, notifications);
        }

        public async Task DisableNotificationsAsync(string channelId)
        {
            if (!await IsNotificationsEnabledAsync(channelId))
            {
                return;
            }

            var notifications = await _storage.ReadAsync<NotificationStorageModel>(Consts.StorageNotificationsKey);
            var toRemove = notifications.NotificationsData.Where(n => n.ChannelId.Equals(channelId, StringComparison.OrdinalIgnoreCase)).Single();
            notifications.NotificationsData.Remove(toRemove);

            await _storage.WriteAsync(Consts.StorageNotificationsKey, notifications);
        }
    }
}
