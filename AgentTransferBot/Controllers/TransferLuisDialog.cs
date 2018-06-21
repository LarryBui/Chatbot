using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AgentTransferBot
{
    [Serializable]
    [LuisModel("a03a2c57-46ae-4242-b34a-da2a8ee3bd0b", "2653cebbf0a841cda008423722cef11a")]
    public class TransferLuisDialog : LuisDialog<object>
    {
        private readonly IUserToAgent _userToAgent;

        public TransferLuisDialog(IUserToAgent userToAgent)
        {
            _userToAgent = userToAgent;
        }
        [LuisIntent("")]
        public async Task None(IDialogContext context, LuisResult luisResult)
        {
            await context.PostAsync("I didn't understand you.am Is?");
            await context.PostAsync("You can contact our cusomer care representative by typing \"Connect me with customer care\"");
            context.Done<object>(null);
        }

        [LuisIntent("Help")]
        public async Task AgentTransfer(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult luisResult)
        {
            var activity = await message;
            var agent = await _userToAgent.IntitiateConversationWithAgentAsync(activity as Activity, default(CancellationToken));
            if (agent == null)
                await context.PostAsync("All our customer care representatives are busy at the moment. Please try after some time.");
            context.Done<object>(null);
        }
    }
}
