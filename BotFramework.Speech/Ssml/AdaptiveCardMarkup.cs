using System.Xml.Linq;
using AdaptiveCards;

namespace BotFramework.Speech.Ssml
{
    public class AdaptiveCardMarkup : IMarkup
    {
        private AdaptiveCard adaptiveCard;

        public AdaptiveCardMarkup(AdaptiveCard adaptiveCard)
        {
            this.adaptiveCard = adaptiveCard;
        }

        public XNode ToSsml()
        {
            var paragraph = new ParagraphMarkup();
            foreach (var element in adaptiveCard.Body)
            {
                GetStringFromAdaptiveCardElement(element, paragraph, 0);
            }

            foreach (var action in adaptiveCard.Actions)
            {
                paragraph.AddSentence($"{action.Title}");
            }

            return paragraph.ToSsml();
        }

        private static void GetStringFromAdaptiveCardElement(CardElement element, ParagraphMarkup builder, int depth)
        {
            switch (element.Type)
            {
                case "TextBlock":
                    TextBlock tb = element as TextBlock;
                    builder.AddSentence($"{tb?.Text}");
                    break;
                case "ColumnSet":
                    ColumnSet cs = element as ColumnSet;
                    foreach (var childColumn in cs.Columns)
                    {
                        GetStringFromAdaptiveCardElement(childColumn, builder, depth + 1);
                    }
                    break;
                case "Column":
                    Column col = element as Column;
                    foreach (var childElement in col.Items)
                    {
                        GetStringFromAdaptiveCardElement(childElement, builder, depth + 1);
                    }
                    break;
                case "Container":
                    Container cont = element as Container;
                    foreach (var childElement in cont.Items)
                    {
                        GetStringFromAdaptiveCardElement(childElement, builder, depth + 1);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
