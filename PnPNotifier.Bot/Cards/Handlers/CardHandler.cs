using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;

namespace PnPNotifier.Bot.Cards.Handlers
{
    public abstract class CardHandler
    {
        public abstract Task ExecuteAsync(CardPayload payload, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken);
    }
}
