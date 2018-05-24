using Microsoft.Bot.Connector.DirectLine;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace BotFramework.Speech
{
    public class BotSpeechClient
    {
        private BingSpeech bingSpeech;

        public SynthesisProperties Properties { get; }

        public Action<Stream> OnPlayStream;

        public BotSpeechClient(SynthesisProperties properties)
        {
            bingSpeech = new BingSpeech();
            bingSpeech.OnAudioAvailable += Cortana_OnAudioAvailable;
            bingSpeech.OnError += Cortana_OnError;
            Properties = properties;

            Validate(properties);
        }

        private void Validate(SynthesisProperties properties)
        {
            if (!string.IsNullOrEmpty(properties.VoiceName))
            {
                if (!properties.VoiceName.Contains(properties.Locale))
                {
                    throw new Exception("Voicename has different Locale than the speech client");
                }

                if (!properties.VoiceName.Contains(properties.Gender.ToString()))
                {
                    throw new Exception("Voicename has different Gender than the speech client");
                }
            }
        }

        private void Cortana_OnError(object sender, DataEventArgs<Exception> e)
        {

        }

        private void Cortana_OnAudioAvailable(object sender, DataEventArgs<Stream> e)
        {
            OnPlayStream?.Invoke(e.Data);
            e.Data.Dispose();
        }

        public void Speak(Activity activity)
        {
            bingSpeech.Speak(activity, CancellationToken.None, Properties).Wait();
        }

        public void Speak(string text)
        {
            // create a dummy activity
            var activity = new Activity(text: text);
            bingSpeech.Speak(activity, CancellationToken.None, Properties).Wait();
        }
    }
}
