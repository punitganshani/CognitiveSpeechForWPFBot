using BotFramework.Speech;
using Microsoft.Bot.Connector.DirectLine;
using Microsoft.CognitiveServices.SpeechRecognition;
using System;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SpeechClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MicrophoneRecognitionClient clientUserSpeech;
        private DirectLineClient clientBot;
        private Microsoft.Bot.Connector.DirectLine.Conversation conversation;
        private BackgroundWorker worker = new BackgroundWorker();
        private BotSpeechClient clientBotSpeech;

        public const int MinTextWidth = 700;
        public const int MaxTextWidth = 750;

        public MainWindow()
        {
            InitializeComponent();

            this.Closed += MainWindow_Closed;

            InitSpeech();
            InitBotConnection();
            InitSynthesizer();
        }

        private void InitSynthesizer()
        {
            // refer to https://docs.microsoft.com/en-us/azure/cognitive-services/speech/api-reference-rest/bingvoiceoutput
            var properties = new SynthesisProperties
            {
                SpeechUri = "https://westus.tts.speech.microsoft.com/cognitiveservices/v1",
                AuthenticationUri = "https://api.cognitive.microsoft.com/sts/v1.0/issueToken",
                SubscriptionKey = ConfigurationManager.AppSettings["CognitiveKey"],
                Gender = SynthesisProperties.SpeechGender.Male,
                Locale = ConfigurationManager.AppSettings["Locale"],
                VoiceName = "Microsoft Server Speech Text to Speech Voice (en-US, BenjaminRUS)",
                OutputFormat = AudioFormat.Riff16Khz16BitMonoPcm,
                AppId = "BB9666309E9D43248A6797CAF61D43D4", // random GUID with no dashes
                AppInstanceId = "D61A1A66964B468E9463DEB5B0B90CE1", // random GUID with no dashes
                AppName = "WPFBot"
            };

            clientBotSpeech = new BotSpeechClient(properties)
            {
                OnPlayStream = stream =>
                {
                    // Riff player
                    var player = new SoundPlayer(stream);
                    player.Play();

                    //StartRecognition();
                }
            };
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            worker.CancelAsync();
        }

        private void InitBotConnection()
        {
            //TODO: you should ideally use DirectLine Secret instead of Token
            clientBot = new DirectLineClient(ConfigurationManager.AppSettings["DirectLineToken"]);
            conversation = clientBot.Conversations.StartConversation();

            worker = new BackgroundWorker() { WorkerSupportsCancellation = true };
            worker.DoWork += Worker_DoWork;

            worker.RunWorkerAsync();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                string watermark = null;
                while (true)
                {
                    if (worker.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }

                    var activitySet = clientBot.Conversations.GetActivities(conversation.ConversationId, watermark);
                    watermark = activitySet?.Watermark;

                    var activities = (from x in activitySet.Activities
                                      select x).ToList();
                    if (activities.Any())
                    {
                        var botReply = activities.Where(x => x.From.Id != Environment.UserName)
                                        .ToList();

                        // botReply received
                        if (botReply.Any())
                        {
                            Dispatcher.Invoke(() =>
                            {
                                foreach (var item in botReply)
                                {
                                    lstResponse.Items.Insert(0, item.Text);
                                    clientBotSpeech.Speak(item);
                                }
                            });
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }


        private void InitSpeech()
        {
            clientUserSpeech = SpeechRecognitionServiceFactory.CreateMicrophoneClient(SpeechRecognitionMode.ShortPhrase,
                ConfigurationManager.AppSettings["Locale"] ?? "en-US",
                ConfigurationManager.AppSettings["CognitiveKey"]);

            // Event handlers for speech recognition results
            clientUserSpeech.OnMicrophoneStatus += this.OnMicrophoneStatus;
            clientUserSpeech.OnPartialResponseReceived += this.OnPartialResponseReceivedHandler;
            clientUserSpeech.OnResponseReceived += OnMicShortPhraseResponseReceivedHandler;
            clientUserSpeech.OnConversationError += OnConversationErrorHandler;
        }

        private void OnMicShortPhraseResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                Debug.WriteLine("--- OnMicShortPhraseResponseReceivedHandler ---");

                // we got the final result, so it we can end the mic reco.  No need to do this
                // for dataReco, since we already called endAudio() on it as soon as we were done
                // sending all the data.
                this.clientUserSpeech.EndMicAndRecognition();

                btnMic.IsEnabled = true;
                btnSend.Visibility = Visibility.Visible;
                dockText.Width = MinTextWidth; // this should be moved to MVVM

                var text = WriteResponseResult(e);
                if (text == null)
                {
                    HandleCouldNotUnderstand();
                }
                else
                {
                    SendTextAsync(text);
                }
            }));
        }

        private void HandleCouldNotUnderstand()
        {
            clientBotSpeech.Speak("Sorry, I couldn't understand. Can you please try again?");

            StartRecognition();
        }

        private string WriteResponseResult(SpeechResponseEventArgs e)
        {
            string value = string.Empty;
            if (e.PhraseResponse.Results.Length == 0)
            {
                Debug.WriteLine("No phrase response is available.");
            }
            else
            {
                // Ignore the Low Confidence scores
                var currentResponse = e.PhraseResponse.Results
                                            .Where(x => x.Confidence != Confidence.Low && !string.IsNullOrEmpty(x.DisplayText))
                                            .ToList()
                                            .FirstOrDefault(); // may be no match?

                value = currentResponse?.DisplayText;
                Debug.WriteLine($"Confidence={currentResponse?.Confidence}, Text=\"{currentResponse?.DisplayText}\"");
            }

            return value;
        }

        private void btnMic_Click(object sender, RoutedEventArgs e)
        {
            StartRecognition();
        }

        private void StartRecognition()
        {
            clientUserSpeech.StartMicAndRecognition();

            Dispatcher.Invoke(() =>
            {
                btnMic.IsEnabled = false;
                btnSend.Visibility = Visibility.Hidden;
                dockText.Width = MaxTextWidth; // this should be moved to MVVM

                txtText.Text = "Listening..";
            });
        }

        private void OnMicrophoneStatus(object sender, MicrophoneEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                Debug.WriteLine("********* Microphone status: {0} *********", e.Recording);
                if (e.Recording)
                {
                    Debug.WriteLine("Please start speaking.");
                    txtText.Text = "Listening..";
                }
            });
        }

        private void OnConversationErrorHandler(object sender, SpeechErrorEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                btnMic.IsEnabled = true;
            });

            Debug.WriteLine("--- Error received by OnConversationErrorHandler() ---");
            Debug.WriteLine("Error code: {0}", e.SpeechErrorCode.ToString());
            Debug.WriteLine("Error text: {0}", e.SpeechErrorText);
        }


        private void OnPartialResponseReceivedHandler(object sender, PartialSpeechResponseEventArgs e)
        {
            Debug.WriteLine("--- Partial result received by OnPartialResponseReceivedHandler() ---");
            Debug.WriteLine(e.PartialResult);

            Dispatcher.Invoke(() =>
            {
                txtText.Text = e.PartialResult;
            });
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            SendTextAsync(txtText.Text);
        }

        private async Task SendTextAsync(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                lstMessages.Items.Insert(0, value);
                txtText.Text = string.Empty;

                // send the message to bot
                Activity userMessage = new Activity
                {
                    From = new ChannelAccount(id: Environment.UserName, name: "Me"),
                    Text = value,
                    Type = ActivityTypes.Message,
                };

                var output = await clientBot.Conversations.PostActivityAsync(conversation.ConversationId, userMessage);
            }
            else
            {
                txtText.Text = string.Empty;
            }
        }

        private void btnHelp_Click(object sender, RoutedEventArgs e)
        {
            SendTextAsync("help");
        }

        private void txtText_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                SendTextAsync(txtText.Text);
            }
        }
    }
}
