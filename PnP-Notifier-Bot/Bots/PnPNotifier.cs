using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using PnPNotifierBot.Cards;
using PnPNotifierBot.Cards.Handlers;
using PnPNotifierBot.Commands;

namespace PnPNotifierBot.Bots
{
    public class PnPNotifier : ActivityHandler
    {
        private readonly CommandsFactory _commandsFactory;
        private readonly CardsHandlerFactory _cardsHandlerFactory;

        public PnPNotifier(CommandsFactory commandsFactory, CardsHandlerFactory cardsHandlerFactory)
        {
            _commandsFactory = commandsFactory;
            _cardsHandlerFactory = cardsHandlerFactory;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var command = turnContext.Activity.RemoveRecipientMention();
            var value = turnContext.Activity.Value as JObject;

            if (value == null || value[nameof(CardPayload.CardActionType)] == null)
            {
                await OnCommandAsync(command, turnContext, cancellationToken);
            }
            else
            {
                var payload = value.ToObject<CardPayload>();
                await OnCardActionAsync(payload, turnContext, cancellationToken);
            }
        }

        private async Task OnCommandAsync(string command, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var commandHandler = _commandsFactory.ResolveCommandHandler(command);
            await commandHandler.HandleAsync(turnContext, cancellationToken);
        }

        private async Task OnCardActionAsync(CardPayload payload, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var actionHandler = _cardsHandlerFactory.Resolve(payload.CardType);
            await actionHandler.ExecuteAsync(payload, turnContext, cancellationToken);
        }
    }
}
