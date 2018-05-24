using System.Xml.Linq;

namespace BotFramework.Speech.Ssml
{
    internal class TextMarkup : IMarkup
    {
        public TextMarkup(string input)
        {
            Input = input;
        }

        public string Input { get; }

        public XNode ToSsml()
        {
            if (!string.IsNullOrEmpty(Input))
            {
                return new XText(Input);
            }

            return null;
        }
    }
}
