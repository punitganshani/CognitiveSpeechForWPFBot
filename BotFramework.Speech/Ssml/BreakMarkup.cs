using System.Collections.Generic;
using System.Xml.Linq;

namespace BotFramework.Speech.Ssml
{
    public class BreakMarkup : IMarkup
    {
        public string Time { get; set; }
        public string Strength { get; set; }

        public XNode ToSsml()
        {
            List<XAttribute> attributes = new List<XAttribute>();
            if (!string.IsNullOrWhiteSpace(Time))
            {
                attributes.Add(new XAttribute("time", Time));
            }

            if (!string.IsNullOrWhiteSpace(Strength))
            {
                attributes.Add(new XAttribute("strength", Strength));
            }
            return new XElement("break", attributes);
        }
    }
}
