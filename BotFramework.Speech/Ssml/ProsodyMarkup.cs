using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BotFramework.Speech.Ssml
{
    public class ProsodyMarkup : IMarkup
    {
        public string Rate { get; set; }
        public string Pitch { get; set; }
        public string Volume { get; set; }
        private List<IMarkup> Markups { get; set; } = new List<IMarkup>();

        public ProsodyMarkup(params IMarkup[] markups)
        {
            Markups = markups.ToList();
        }

        public XNode ToSsml()
        {
            List<XObject> attributes = new List<XObject>();

            if (!string.IsNullOrWhiteSpace(Rate))
            {
                attributes.Add(new XAttribute("rate", Rate));
            }

            if (!string.IsNullOrWhiteSpace(Pitch))
            {
                attributes.Add(new XAttribute("pitch", Pitch));
            }

            if (!string.IsNullOrWhiteSpace(Volume))
            {
                attributes.Add(new XAttribute("volume", Volume));
            }

            return new XElement("prosody", attributes.Concat(Markups.Select(e => e.ToSsml())));
        }

        public void AddMarkup(IMarkup markup)
        {
            if (markup != null)
            {
                Markups.Add(markup);
            }
        }
    }
}
