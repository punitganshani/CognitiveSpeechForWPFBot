using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;

namespace BotFramework.Speech.Handlers
{
    public interface IAttachmentHandler<T>
    {
        void Process(T input, PromptBuilder builder);

        void Process(JObject input, PromptBuilder builder);
    }
}
