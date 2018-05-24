using Microsoft.Bot.Connector.DirectLine;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
namespace BotFramework.Speech
{
    internal class BingSpeech
    {
        /// <summary>
        /// Generates SSML.
        /// </summary>
        /// <param name="locale">The locale.</param>
        /// <param name="gender">The gender.</param>
        /// <param name="name">The voice name.</param>
        /// <param name="text">The text input.</param>
        private string GenerateSsml(string locale, string gender, string name, Activity activity)
        {

            var ssmlDoc = new XDocument(
                              new XElement("speak",
                                  new XAttribute("version", "1.0"),
                                  new XAttribute(XNamespace.Xml + "lang", "en-US"),
                                  new XElement("voice",
                                      new XAttribute(XNamespace.Xml + "lang", locale),
                                      new XAttribute(XNamespace.Xml + "gender", gender),
                                      new XAttribute("name", name),
                                      activity.Text)));
            return ssmlDoc.ToString();
        }

        private HttpClient client;
        private HttpClientHandler handler;

        /// <summary>
        /// Initializes a new instance of the <see cref="BingSpeech"/> class.
        /// </summary>
        public BingSpeech()
        {
            var cookieContainer = new CookieContainer();
            handler = new HttpClientHandler() { CookieContainer = new CookieContainer(), UseProxy = false };
            client = new HttpClient(handler);
        }

        ~BingSpeech()
        {
            client.Dispose();
            handler.Dispose();
        }

        /// <summary>
        /// Called when a TTS request has been completed and audio is available.
        /// </summary>
        public event EventHandler<DataEventArgs<Stream>> OnAudioAvailable;

        /// <summary>
        /// Called when an error has occured. e.g this could be an HTTP error.
        /// </summary>
        public event EventHandler<DataEventArgs<Exception>> OnError;

        /// <summary>
        /// Sends the specified text to be spoken to the TTS service and saves the response audio to a file.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task</returns>
        internal Task Speak(Activity activity, CancellationToken cancellationToken, SynthesisProperties inputOptions)
        {
            var activityConverter = new ActivityConverter(inputOptions);

            client.DefaultRequestHeaders.Clear();
            foreach (var header in inputOptions.GetHeaders())
            {
                client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }

            var genderValue = inputOptions.Gender.ToString();

            var request = new HttpRequestMessage(HttpMethod.Post, new Uri(inputOptions.SpeechUri))
            {
                Content = new StringContent(activityConverter.ToSsml(activity))
            };

            var httpTask = client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            Console.WriteLine("Response status code: [{0}]", httpTask.Result.StatusCode);

            var saveTask = httpTask.ContinueWith(
                async (responseMessage, token) =>
                {
                    try
                    {
                        if (responseMessage.IsCompleted && responseMessage.Result != null && responseMessage.Result.IsSuccessStatusCode)
                        {
                            var httpStream = await responseMessage.Result.Content.ReadAsStreamAsync().ConfigureAwait(false);
                            AudioAvailable(new DataEventArgs<Stream>(httpStream));
                        }
                        else
                        {
                            Error(new DataEventArgs<Exception>(new Exception(String.Format("Service returned {0}", responseMessage.Result.StatusCode))));
                        }
                    }
                    catch (Exception e)
                    {
                        Error(new DataEventArgs<Exception>(e.GetBaseException()));
                    }
                    finally
                    {
                        responseMessage.Dispose();
                        request.Dispose();
                    }
                },
                TaskContinuationOptions.AttachedToParent,
                cancellationToken);

            return saveTask;
        }

        /// <summary>
        /// Called when a TTS requst has been successfully completed and audio is available.
        /// </summary>
        private void AudioAvailable(DataEventArgs<Stream> e)
        {
            this.OnAudioAvailable?.Invoke(this, e);
        }

        /// <summary>
        /// Error handler function
        /// </summary>
        /// <param name="e">The exception</param>
        private void Error(DataEventArgs<Exception> e)
        {
            this.OnError?.Invoke(this, e);
        }
    }
}
