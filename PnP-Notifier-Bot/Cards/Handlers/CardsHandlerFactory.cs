using Microsoft.Extensions.DependencyInjection;
using System;

namespace PnPNotifierBot.Cards.Handlers
{
    public class CardsHandlerFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public CardsHandlerFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public CardHandler Resolve(CardType cardType)
        {
            switch (cardType)
            {
                case CardType.ConfigurePnPNotifications: return _serviceProvider.GetRequiredService<ConfigurePnPNotificationsCardHandler>();
                default: throw new Exception("Unable to resolve card handler with type " + cardType);
            }
        }
    }
}
