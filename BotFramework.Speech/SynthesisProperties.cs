using System;
using System.Collections.Generic;

namespace BotFramework.Speech
{
    public class SynthesisProperties
    {
        public string SubscriptionKey { get; set; }
        public string SpeechUri { get; set; }
        public string AuthenticationUri { get; set; }
        public SpeechGender Gender { get; set; }
      
        public string Locale { get; set; }
        public AudioFormat OutputFormat { get; set; }
        public string VoiceName { get; set; }
        public string AppId { get; set; }
        public string AppInstanceId { get; set; }
        public string AppName { get; set; }

        public IEnumerable<KeyValuePair<string, string>> GetHeaders()
        {
            var output = new List<KeyValuePair<string, string>>
                    {
                        new KeyValuePair<string, string>("Content-Type", "application/ssml+xml")
                    };

            string outputFormat;

            switch (OutputFormat)
            {
                case AudioFormat.Riff16Khz16BitMonoPcm:
                    outputFormat = "riff-16khz-16bit-mono-pcm";
                    break;

                case AudioFormat.Riff16Khz16KbpsMonoSiren:
                    outputFormat = "riff-16khz-16kbps-mono-siren";
                    break;

                case AudioFormat.Riff24Khz16BitMonoPcm:
                    outputFormat = "riff-24khz-16bit-mono-pcm";
                    break;

                default:
                    outputFormat = "riff-16khz-16bit-mono-pcm";
                    break;
            }

            output.Add(new KeyValuePair<string, string>("X-Microsoft-OutputFormat", outputFormat));
            // authorization Header
            output.Add(new KeyValuePair<string, string>("Authorization", "bearer " + GetAuthorizationToken()));
            // App ID
            output.Add(new KeyValuePair<string, string>("X-Search-AppId", AppId));
            // App Instance ID
            output.Add(new KeyValuePair<string, string>("X-Search-ClientID", AppInstanceId));
            // App name should not be >255 characters
            output.Add(new KeyValuePair<string, string>("User-Agent", AppName));

            return output;
        }

        private string authenticationToken;
        private Authentication authentication;

        private string GetAuthorizationToken()
        {
            if (!string.IsNullOrEmpty(authenticationToken))
            {
                return authenticationToken;
            }

            authentication = new Authentication(this.AuthenticationUri, this.SubscriptionKey);
            try
            {
                authenticationToken = authentication.GetAccessToken();
                return authenticationToken;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        ~SynthesisProperties()
        {
            authentication = null;
        }

        public enum SpeechGender
        {
            Female,
            Male
        }
    }

}
