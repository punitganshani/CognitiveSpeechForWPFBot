using AdaptiveCards;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace BotFramework.Speech.Handlers
{
    public class AdaptiveCardHandler : IAttachmentHandler<AdaptiveCard>
    {
        public void Process(AdaptiveCard ac, PromptBuilder builder)
        {
            foreach (var element in ac.Body)
            {
                GetStringFromAdaptiveCardElement(element, builder, 0);
            }

            foreach (var action in ac.Actions)
            {
                builder.AppendText($"{action.Title}");
            }
        }

        public void Process(JObject input, PromptBuilder builder)
        {
            throw new NotImplementedException();
        }

        private static void GetStringFromAdaptiveCardElement(CardElement element, PromptBuilder builder, int depth)
        {
            switch (element.Type)
            {
                case "TextBlock":
                    TextBlock tb = element as TextBlock;
                    string indent = new String(' ', depth);
                    builder.AppendText($"{indent}{tb?.Text}");
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
