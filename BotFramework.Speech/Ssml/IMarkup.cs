using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BotFramework.Speech.Ssml
{
    public interface IMarkup
    {
        XNode ToSsml();
    }
}
