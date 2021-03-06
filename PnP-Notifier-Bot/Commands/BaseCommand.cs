using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;

namespace PnPNotifierBot.Commands
{
    public abstract class BaseCommand
    {
        public abstract Task HandleAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken);
    }
}
