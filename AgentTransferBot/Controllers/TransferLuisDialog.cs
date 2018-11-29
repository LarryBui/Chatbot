using AdaptiveCards;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace AgentTransferBot
{
    [Serializable]
    [LuisModel("b88a797a-4876-4bc5-a429-6a7ea0f6ca9f", "2653cebbf0a841cda008423722cef11a")]
    public class TransferLuisDialog : LuisDialog<object>
    {
        private readonly IUserToAgent _userToAgent;

        public TransferLuisDialog(IUserToAgent userToAgent)
        {

            _userToAgent = userToAgent;
        }
        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult luisResult)
        {
            await context.PostAsync("Sorry, I wasn't trained for this.");
            await context.PostAsync("Please rephrase your question or type \"Customer service\" to talk to agent Larry");
            context.Done<object>(null);
        }

        [LuisIntent("Greeting")]
        public async Task Greetings(IDialogContext context, LuisResult luisResult)
        {
            if(luisResult.TopScoringIntent.Score < 0.5)
            {
                await None(context, luisResult);
            } else
            {
                await context.PostAsync("Hello");
            }
            context.Done<object>(null);
        }
        [LuisIntent("AgentTransfer")]
        public async Task AgentTransfer(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult luisResult)
        {
            if (luisResult.TopScoringIntent.Score < 0.5)
            {
                await None(context, luisResult);
            }
            else
            {
                var activity = await message;
                var agent = await _userToAgent.IntitiateConversationWithAgentAsync(activity as Activity, default(CancellationToken));
                if (agent == null)
                    await context.PostAsync("All our customer care representatives are busy at the moment. Please try after some time.");
            }
            context.Done<object>(null);
        }

        [LuisIntent("Menu")]
        public async Task ShowMenu(IDialogContext context, IAwaitable<IMessageActivity> message, LuisResult luisResult)
        {
            if (luisResult.TopScoringIntent.Score < 0.5)
            {
                await None(context, luisResult);
            }
            else
            {
                Activity reply = ((Activity)context.Activity).CreateReply();
                string json = File.ReadAllText(HttpContext.Current.Request.MapPath("~\\AdaptiveCards\\MyCard.json"));
                // use Newtonsofts JsonConvert to deserialized the json into a C# AdaptiveCard object
                AdaptiveCards.AdaptiveCard card = JsonConvert.DeserializeObject<AdaptiveCards.AdaptiveCard>(json);
                // put the adaptive card as an attachment to the reply message
                reply.Attachments.Add(new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = card
                });
                await context.PostAsync(reply);
            }
            context.Done<object>(null);
        }
    }
}
