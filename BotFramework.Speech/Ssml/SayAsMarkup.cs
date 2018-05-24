using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BotFramework.Speech.Ssml
{
    public class SayAsMarkup : IMarkup
    {
        public string Text { get; set; }
        public string Format { get; set; }

        public InterpretAs InterpretAs { get; set; }

        public SayAsMarkup(string text)
        {
            Text = text;
        }

        public XNode ToSsml()
        {
            List<XObject> objects = new List<XObject>
            {
                new XText(Text),
                new XAttribute("interpret-as", GetTextFor(InterpretAs))
            };

            if (!string.IsNullOrWhiteSpace(Format))
            {
                objects.Add(new XAttribute("format", Format));
            }

            return new XElement("say-as", objects);
        }

        private string GetTextFor(InterpretAs interpretAs)
        {
            switch (interpretAs)
            {
                case InterpretAs.SpellOut:
                    return "spell-out";
                default:
                    return interpretAs.ToString().ToLowerInvariant();
            }
        }
    }
}
