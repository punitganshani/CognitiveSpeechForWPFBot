using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BotFramework.Speech.Ssml
{
    public class ParagraphMarkup : IMarkup
    {
        private List<SentenceMarkup> Sentences = new List<SentenceMarkup>();

        public ParagraphMarkup()
        {

        }

        public ParagraphMarkup(params SentenceMarkup[] sentences)
        {
            this.Sentences = sentences?.ToList();
        }
        public XNode ToSsml()
        {
            return new XElement("p", Sentences.Select(e => e.ToSsml()));
        }

        public void AddSentence(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                Sentences.Add(new SentenceMarkup(text));
            }
        }
    }
}