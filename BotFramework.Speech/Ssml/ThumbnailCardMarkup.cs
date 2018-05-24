using System.Xml.Linq;
using Microsoft.Bot.Connector.DirectLine;

namespace BotFramework.Speech.Ssml
{
    public class ThumbnailCardMarkup : IMarkup
    {
        private ThumbnailCard thumbnailCard;

        public ThumbnailCardMarkup(ThumbnailCard thumbnailCard)
        {
            this.thumbnailCard = thumbnailCard;
        }

        public XNode ToSsml()
        {
            return new XText(thumbnailCard.Title);
        }
    }
}
