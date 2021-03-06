using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.BotFramework;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using PnPNotifierBot.Bots;
using PnPNotifierBot.Cards.Handlers;
using PnPNotifierBot.Cards.Managers;
using PnPNotifierBot.Commands;
using PnPNotifierBot.Common;
using PnPNotifierBot.Config;

namespace PnPNotifierBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
            services.AddTransient<IBot, PnPNotifier>();
            services.AddTransient<CommandsFactory>();
            services.AddTransient<CardsHandlerFactory>();
            services.AddScoped<NotificationsManager>();
            services.AddScoped<ConfigureNotificationsCardManager>();
            

            services.AddTransient<ConfigureNotificationsCommand>();
            services.AddTransient<ConfigurePnPNotificationsCardHandler>();

            services.Configure<BotCredentials>(Configuration.GetSection(BotCredentials.SectionName));
            var cosmosConfig = Configuration.GetSection("CosmosDb").Get<CosmosDbPartitionedStorageOptions>();
            cosmosConfig.CompatibilityMode = false;

            var jsonSerializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None,
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore
            });

            services.AddScoped<IStorage>((provider) => new CosmosDbPartitionedStorage(cosmosConfig, jsonSerializer));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });
        }
    }
}
