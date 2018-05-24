using System.Collections.Generic;
using System.Xml.Linq;

namespace BotFramework.Speech.Ssml
{
    public class EmphasisMarkup : IMarkup
    {
        public string Text { get; }
        public string Level { get; set; }

        public EmphasisMarkup(string text)
        {
            Text = text;
        }
     
        public XNode ToSsml()
        {
            List<XObject> objects = new List<XObject>();

            if (!string.IsNullOrWhiteSpace(Level))
            {
                objects.Add(new XAttribute("level", Level));
            }

            objects.Add(new XText(Text));

            return new XElement("emphasis", objects);
        }
    }
}
