using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;

namespace PnPNotifierBot.Cards.Handlers
{
    public abstract class CardHandler
    {
        public abstract Task ExecuteAsync(CardPayload payload, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken);
    }
}
