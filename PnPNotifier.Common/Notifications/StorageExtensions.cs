using Microsoft.Bot.Builder;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PnPNotifier.Common.Notifications
{
    public static class StorageExtensions
    {
        public static async Task<T> ReadAsync<T>(this IStorage storage, string key) where T: class, new()
        {
            var result = (await storage.ReadAsync(new string[] { key })).SingleOrDefault();
            if(result.Equals(default(KeyValuePair<string, object>)))
            {
                return null;
            }

            var value = result.Value as JObject;

            return value.ToObject<T>();
        }

        public static async Task WriteAsync<T>(this IStorage storage, string key, T data)
        {
            await storage.WriteAsync(new Dictionary<string, object> { { key, data } });
        }
    }
}
