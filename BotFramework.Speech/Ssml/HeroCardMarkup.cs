using System.Linq;
using System.Xml.Linq;
using Microsoft.Bot.Connector.DirectLine;

namespace BotFramework.Speech.Ssml
{
    public class HeroCardMarkup : IMarkup
    {
        private HeroCard hero;

        public HeroCardMarkup(HeroCard heroCard)
        {
            this.hero = heroCard;
        }

        public XNode ToSsml()
        {
            string text = string.Empty;
            if (!string.IsNullOrEmpty(hero.Title) && !string.IsNullOrEmpty(hero.Text))
            {
                text = $"{hero.Title}";
            }
            else if (!string.IsNullOrEmpty(hero.Title))
            {
                text = $"{hero.Title}";
            }
            else if (!string.IsNullOrEmpty(hero.Text))
            {
                text = $"{hero.Text}";
            }

            XElement element = new XElement("paragraph", new XText(text));

            if (hero.Buttons.Any())
            {
                element.Add(new BreakMarkup().ToSsml());
                element.Add(new TextMarkup("You can say,").ToSsml());
                //builder.AppendBreak();
                //builder.AppendText("You can say,");
                //builder.StartSentence();

                for (int i = 0; i < hero.Buttons.Count; i++)
                {
                    element.Add(new TextMarkup($"{hero.Buttons[i].Title}").ToSsml());
                    //builder.AppendText($"{hero.Buttons[i].Title}");

                    if (i != hero.Buttons.Count - 1)
                    {
                        element.Add(new TextMarkup(", or ").ToSsml());
                        //builder.AppendText(", or ");
                    }
                }

                //builder.EndSentence();
            }

            return element;
        }
    }
}
