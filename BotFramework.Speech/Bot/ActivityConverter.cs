using Microsoft.Bot.Connector.DirectLine;
using Newtonsoft.Json;
using AdaptiveCards;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using BotFramework.Speech.Ssml;
using System.Linq;

namespace BotFramework.Speech
{
    internal class ActivityConverter
    {
        private SynthesisProperties properties;

        public ActivityConverter(SynthesisProperties properties)
        {
            this.properties = properties;
        }

        internal string ToSsml(Activity activity)
        {
            var voiceElement = new XElement("voice",
                                      new XAttribute(XNamespace.Xml + "lang", properties.Locale),
                                      new XAttribute(XNamespace.Xml + "gender", properties.Gender.ToString()),
                                      new XAttribute("name", properties.VoiceName));

            var ssmlDoc = new XDocument(
                              new XElement("speak",
                                  new XAttribute("version", "1.0"),
                                  new XAttribute(XNamespace.Xml + "lang", properties.Locale),
                                  voiceElement));

            var markups = GetMarkups(activity);
            markups.ForEach(x=> voiceElement.Add(x.ToSsml()));

            return ssmlDoc.ToString();
        }

        private List<IMarkup> GetMarkups(Activity activity)
        {
            List<IMarkup> markups = new List<IMarkup>
            {
                new TextMarkup(GetMainText(activity))
            };

            if (activity.Attachments != null && activity.Attachments.Any())
            {
                // add a pause
                markups.Add(new BreakMarkup());

                foreach (var attachment in activity.Attachments)
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
                                markups.Add(new ThumbnailCardMarkup(thumbnailCard));
                            }
                            else if (attachment.Content is JObject)
                            {
                                var jObject = (JObject)attachment.Content;
                                var card = jObject.ToObject<ThumbnailCard>();
                                markups.Add(new ThumbnailCardMarkup(card));
                            }
                            break;

                        case "application/vnd.microsoft.card.hero":
                            var heroCard = JsonConvert.DeserializeObject<HeroCard>(attachment.Content.ToString());
                            markups.Add(new HeroCardMarkup(heroCard));
                            break;

                        case "application/vnd.microsoft.card.adaptive":
                            AdaptiveCard adaptiveCard = JsonConvert.DeserializeObject<AdaptiveCard>(attachment.Content.ToString());
                            markups.Add(new AdaptiveCardMarkup(adaptiveCard)); 
                            break;
                    }
                }
            }

            return markups;
        }

        private string GetMainText(Activity activity)
        {
            if (!string.IsNullOrEmpty(activity.Speak))
            {
                return activity.Speak;
            }
            else if (!string.IsNullOrEmpty(activity.Text) && !activity.Text.Contains("://"))
            {
                return activity.Text;
            }

            return null;
        }
    }
}
