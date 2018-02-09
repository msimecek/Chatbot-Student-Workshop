using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AnoNeBot.Services;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace AnoNeBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            if (!activity.Text.EndsWith("?"))
            {
                await context.PostAsync("To je zajímavé, ale dokud mi nepoložíš otázku, tak ti nemohu pomoci...");
            }
            else
            {
                var yesNo = await YesNoService.GetYesNoAsync(true);

                var reply = activity.CreateReply();
                var card = new HeroCard(yesNo.Answer)
                {
                    Images = new List<CardImage>()
            {
                new CardImage(yesNo.Image)
            }
                };

                reply.Attachments.Add(card.ToAttachment());
                await context.PostAsync(reply);
            }

            context.Wait(MessageReceivedAsync);
        }
    }
}