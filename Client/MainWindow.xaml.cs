using Microsoft.CognitiveServices.SpeechRecognition;
using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows;

namespace SpeechClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MicrophoneRecognitionClient _client;

        public const int MinTextWidth = 700;
        public const int MaxTextWidth = 750;


        public MainWindow()
        {
            InitializeComponent();

            _client = SpeechRecognitionServiceFactory.CreateMicrophoneClient(SpeechRecognitionMode.ShortPhrase,
                ConfigurationManager.AppSettings["Locale"] ?? "en-US",
                ConfigurationManager.AppSettings["CognitiveKey"]);

            // Event handlers for speech recognition results
            _client.OnMicrophoneStatus += this.OnMicrophoneStatus;
            _client.OnPartialResponseReceived += this.OnPartialResponseReceivedHandler;
            _client.OnResponseReceived += OnMicShortPhraseResponseReceivedHandler;
            _client.OnConversationError += this.OnConversationErrorHandler;
        }
        private void OnMicShortPhraseResponseReceivedHandler(object sender, SpeechResponseEventArgs e)
        {
            Dispatcher.Invoke((Action)(() =>
            {
                Debug.WriteLine("--- OnMicShortPhraseResponseReceivedHandler ---");

                // we got the final result, so it we can end the mic reco.  No need to do this
                // for dataReco, since we already called endAudio() on it as soon as we were done
                // sending all the data.
                this._client.EndMicAndRecognition();

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
                    SendText(text);
                }
            }));
        }

        private void HandleCouldNotUnderstand()
        {
            SpeechSynthesizer synthesizer = new SpeechSynthesizer
            {
                Volume = 100,  // 0...100
                Rate = 0    // -10...10
            };

            synthesizer.Speak("Sorry, I couldn't understand. Can you please try again?");
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
            this._client.StartMicAndRecognition();

            btnMic.IsEnabled = false;
            btnSend.Visibility = Visibility.Hidden;
            dockText.Width = MaxTextWidth; // this should be moved to MVVM

            txtText.Text = string.Empty;
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
            SendText(null);
        }

        private void SendText(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                lstMessages.Items.Insert(0, value);
                txtText.Text = string.Empty;
            }
            else
            {
                txtText.Text = string.Empty;
            }
        }
    }
}
