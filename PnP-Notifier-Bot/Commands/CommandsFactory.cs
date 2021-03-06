using Microsoft.Extensions.DependencyInjection;
using System;

namespace PnPNotifierBot.Commands
{
    public class CommandsFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public CommandsFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public BaseCommand ResolveCommandHandler(string command)
        {
            switch (command)
            {
                case "Configure PnP notifications": return _serviceProvider.GetRequiredService<ConfigureNotificationsCommand>();

                default: throw new Exception($"Unable to find a command with key '{command}'");
            }
        }
    }
}
