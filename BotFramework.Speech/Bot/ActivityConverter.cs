using Microsoft.Bot.Connector.DirectLine;
using System.Speech.Synthesis;
using Newtonsoft.Json;
using AdaptiveCards;
using BotFramework.Speech.Handlers;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace BotFramework.Speech
{
    internal class ActivityConverter
    {
        private SynthesisProperties inputOptions;

        public ActivityConverter(SynthesisProperties inputOptions)
        {
            this.inputOptions = inputOptions;
        }

        internal string ToSsml(Activity activity)
        {
            var builder = new PromptBuilder();
            builder.StartVoice(inputOptions.VoiceName);

            #region Voice
            if (!string.IsNullOrEmpty(activity.Speak))
            {
                builder.AppendText(activity.Speak);
            }
            else if (!string.IsNullOrEmpty(activity.Text) && !activity.Text.Contains("://"))
            {
                builder.AppendText(activity.Text);
            }

            HandleAttachments(activity, builder);
            #endregion

            builder.EndVoice();

            var output = builder.ToXml();
            Debug.WriteLine(output);
            return output;
        }

        private void HandleAttachments(Activity recordActivity, PromptBuilder builder)
        {
            if (recordActivity.Attachments == null)
                return;

            if (recordActivity.Attachments.Count <= 0)
                return;

            builder.AppendBreak();
            foreach (var attachment in recordActivity.Attachments)
            {
                switch (attachment.ContentType)
                {
                    default:
                        // TODO - need to handle all attachment types that the app uses
                        break;

                    case "application/vnd.microsoft.card.thumbnail":
                        ThumbnailCard thumbnailCard = attachment.Content as ThumbnailCard;
                        if (thumbnailCard != null)
                        {
                            new ThumbnailHandler().Process(thumbnailCard, builder);
                        }
                        else if (attachment.Content is JObject)
                        {
                            new ThumbnailHandler().Process((JObject)attachment.Content, builder);
                        }
                        break;

                    case "application/vnd.microsoft.card.hero":
                        var heroCard = JsonConvert.DeserializeObject<HeroCard>(attachment.Content.ToString());
                        new HeroCardHandler().Process(heroCard, builder);
                        break;

                    case "application/vnd.microsoft.card.adaptive":
                        AdaptiveCard adaptiveCard = JsonConvert.DeserializeObject<AdaptiveCard>(attachment.Content.ToString());
                        new AdaptiveCardHandler().Process(adaptiveCard, builder);
                        break;
                }
            }
        }
    }
}
