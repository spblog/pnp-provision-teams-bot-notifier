using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Extensions.DependencyInjection;
using PnPNotifier.Job.Model;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Newtonsoft.Json;
using PnPNotifier.Common.Notifications;
using PnPNotifier.Common.Config;
using PnPNotifier.Job.Notifications;

namespace PnPNotifier.Job
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new HostBuilder();
            var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
            builder
                .UseEnvironment(string.IsNullOrEmpty(environmentName) ? EnvironmentName.Production : environmentName)
                .ConfigureWebJobs(b =>
                {
                    b
                    .AddAzureStorageCoreServices()
                    .AddAzureStorageQueues(c =>
                    {
                        c.MaxDequeueCount = 1;
                    });
                })
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    config
                    .AddJsonFile($"appsettings.{builderContext.HostingEnvironment.EnvironmentName}.json", optional: true)
                    .AddEnvironmentVariables();

                    if (builderContext.HostingEnvironment.IsDevelopment())
                    {
                        config.AddUserSecrets<Program>();
                    }
                })
                .ConfigureLogging((context, logBuilder) =>
                {
                    logBuilder
                    .AddConsole()
                    .AddDebug();
                })
                .ConfigureServices((context, services) => {
                    services.Configure<AzureAdCreds>(context.Configuration.GetSection(AzureAdCreds.SectionName));
                    services.Configure<BotCredentials>(context.Configuration.GetSection(BotCredentials.SectionName));

                    var cosmosConfig = context.Configuration.GetSection("CosmosDb").Get<CosmosDbPartitionedStorageOptions>();
                    cosmosConfig.CompatibilityMode = false;

                    var jsonSerializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.None,
                        MetadataPropertyHandling = MetadataPropertyHandling.Ignore
                    });

                    services.AddScoped<IStorage>((provider) => new CosmosDbPartitionedStorage(cosmosConfig, jsonSerializer));
                    services.AddScoped<NotificationsManager>();
                    services.AddScoped<NotificationCardManager>();
                });
            var host = builder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }
    }
}
