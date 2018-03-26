using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using AnoNeBot.Models;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace AnoNeBot.Dialogs
{
    [Serializable]
    public class WhoIsDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as IMessageActivity;

            if (context.ConversationData.ContainsKey("LastFace"))
            {
                var lastFace = context.ConversationData.GetValue<KeyValuePair<string, string>>("LastFace");

                if (activity.Text.ToLower() == lastFace.Value.ToLower())
                {
                    await context.PostAsync("Correct!");
                }
                else
                {
                    await context.PostAsync("Error! it's " + lastFace.Value);
                }
            }

            await ShowRandomFaceAsync(context);

            context.Wait(MessageReceivedAsync);
        }

        private async Task ShowRandomFaceAsync(IDialogContext context)
        {
            Random rand = new Random();
            var face = PeopleModel.People.ElementAt(rand.Next(0, PeopleModel.People.Count));

            context.ConversationData.SetValue("LastFace", face);

            var root = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority) + "/";
            var card = new ThumbnailCard("Who is it?", images: new List<CardImage>() { new CardImage(root + face.Key) });

            var message = context.MakeMessage();
            message.Attachments.Add(card.ToAttachment());

            await context.PostAsync(message);
        }
    }
}