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
    public class HeroCardHandler : IAttachmentHandler<HeroCard>
    {
        public void Process(HeroCard hero, PromptBuilder builder)
        {
            if (!string.IsNullOrEmpty(hero.Title) && !string.IsNullOrEmpty(hero.Text))
            {
                builder.AppendText($"{hero.Title}");               
            }
            else if (!string.IsNullOrEmpty(hero.Title))
            {
                builder.AppendText($"{hero.Title}");                 
            }
            else if (!string.IsNullOrEmpty(hero.Text))
            {
                builder.AppendText($"{hero.Text}");               
            }

            if (hero.Buttons.Any())
            {
                builder.AppendBreak();
                builder.AppendText("You can say,");
                builder.StartSentence();

                for (int i = 0; i < hero.Buttons.Count; i++)
                {
                    builder.AppendText($"{hero.Buttons[i].Title}");

                    if (i != hero.Buttons.Count - 1)
                    {
                        builder.AppendText(", or ");
                    }
                }

                builder.EndSentence();
            }
        }

        public void Process(JObject input, PromptBuilder builder)
        {
            throw new NotImplementedException();
        }
    }
}
