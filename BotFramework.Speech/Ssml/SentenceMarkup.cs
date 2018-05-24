using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BotFramework.Speech.Ssml
{
    public class SentenceMarkup : IMarkup
    {
        public SentenceMarkup(string text) : this(new TextMarkup(text))
        {

        }

        public SentenceMarkup(params IMarkup[] markups)
        {
            Markups = markups.ToList();
        }

        private List<IMarkup> Markups = new List<IMarkup>();

        public XNode ToSsml()
        {
            return new XElement("s", Markups.Select(x => x.ToSsml()));
        }
    }
}
