using Microsoft.Bot.Connector.DirectLine;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace BotFramework.Speech.Handlers
{
    public class ThumbnailHandler : IAttachmentHandler<ThumbnailCard>
    {
        public void Process(ThumbnailCard tc, PromptBuilder builder)
        {
            builder.AppendText($"{tc.Title}");
        }

        public void Process(JObject jObject, PromptBuilder builder)
        {
            var card = jObject.ToObject<ThumbnailCard>();
            builder.AppendText($"{card.Title}");
            //if (card.Buttons.Any())
            //{
            //    foreach (var buttons in card.Buttons)
            //    {
            //        builder.AppendText($"{buttons.Title}");
            //    }
            //}
        }
    }
}
