using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Globalization;
using System.Media;
using System.Speech.Recognition;
using System.Collections.ObjectModel;
using Microsoft.Speech.Synthesis;

namespace PronunciationConverter2
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private SoundPlayer player;
        private SpeechRecognitionEngine recognizer;
        private SpeechSynthesizer synthesizer;

        private string[] ngWords;
        private ObservableCollection<RecognitionResult> results;

        public MainWindow()
        {
            InitializeComponent();

            ngWords = global::PronunciationConverter2.Properties.Resources.UsePhoneme.Split(',');

            results = new ObservableCollection<RecognitionResult>();
            historyList.ItemsSource = results;

            synthesizer = new SpeechSynthesizer();
            foreach (InstalledVoice voice in synthesizer.GetInstalledVoices())
            {
                selectVoice.Items.Add(voice.VoiceInfo);
            }
            selectVoice.SelectedIndex = 0;
        }

        private void play()
        {
            if (inputFilePath.Text.EndsWith(".wav"))
            {
                player = new SoundPlayer(inputFilePath.Text);
                player.Play();
            }
        }

        private void stop()
        {
            if (player != null)
            {
                player.Stop();
            }
        }

        ///////////////////////////////////////////////////////////////////////
        // Recognize
        ///////////////////////////////////////////////////////////////////////

        private void recognizeFile()
        {
            initRecognizer();

            if (inputFilePath.Text.Length > 0)
            {
                recognizer.SetInputToWaveFile(inputFilePath.Text);
                recognizer.RecognizeAsync(RecognizeMode.Multiple);
            }
        }

        private void startRecognizing()
        {
            initRecognizer();

            recognizer.SetInputToDefaultAudioDevice();
            recognizer.RecognizeAsync(RecognizeMode.Multiple);
        }

        private void initRecognizer()
        {
            recognizer = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US"));
            GrammarBuilder gb = new GrammarBuilder();
            gb.Culture = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
            gb.AppendDictation();
            Grammar g = new Grammar(gb);
            g.Enabled = true;
            recognizer.LoadGrammar(g);
            recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(sre_SpeechRecognized);

            results.Clear();
        }


        void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            results.Add(e.Result);
        }

        ///////////////////////////////////////////////////////////////////////
        // Speak
        ///////////////////////////////////////////////////////////////////////

        private void speak()
        {
            if (results.Count > 0)
            {
                synthesizer.Volume = 100;
                //synthesizer.Rate = -2;
                synthesizer.SelectVoice((selectVoice.SelectedItem as VoiceInfo).Name);

                player = new SoundPlayer();
                player.Stream = new System.IO.MemoryStream();
                synthesizer.SetOutputToWaveStream(player.Stream);

                foreach (RecognitionResult r in results)
                {
                    if (usePhoneme.IsChecked == true)
                        synthesizer.Speak(buildPromptBuilder(r));
                    else
                        synthesizer.Speak(r.Text);
                }

                player.Stream.Position = 0;
                player.Play();
            }
        }

        private void speakToFile()
        {
            MessageBox.Show("Speak to file is not implemented yet.");
        }

        private PromptBuilder buildPromptBuilder(RecognitionResult result)
        {
            PromptBuilder pb = new PromptBuilder();
            VoiceInfo voice = selectVoice.SelectedItem as VoiceInfo;
            pb.AppendSsmlMarkup(String.Format("<voice xml:lang='{0}'>", voice.Culture.ToString()));
            foreach (RecognizedWordUnit w in result.Words)
            {
                //if (ngWords.Contains(w.Text))
                //    pb.AppendTextWithPronunciation(w.Text, w.Pronunciation);
                //else
                //    pb.AppendText(w.Text + " ");
                pb.AppendTextWithPronunciation(w.Text, w.Pronunciation);
            }
            pb.AppendSsmlMarkup("</voice>");
            return pb;
        }

        //private String buildPhonemeElement(RecognizedWordUnit w)
        //{
        //    return String.Format("<phoneme alphabet='ipa' ph='{0}'> {1} </phoneme>", w.Pronunciation, w.Text);
        //}

        ///////////////////////////////////////////////////////////////////////
        // UI event handlers
        ///////////////////////////////////////////////////////////////////////

        private void selectFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.FileName = "";
            dialog.Filter = "wav files|*.wav";
            

            if (dialog.ShowDialog() == true)
            {
                inputFilePath.Text = dialog.FileName;
            }
        }

        private void selectFolderButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            play();
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (inputFromFile.IsChecked == true)
            {
                recognizeFile();
            }
            else if(inputFromMicrophone.IsChecked == true)
            {
                startRecognizing();
            }
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            stop();
        }

        private void speakButton_Click(object sender, RoutedEventArgs e)
        {
            speak();
        }

        private void historyList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                RecognitionResult r = e.AddedItems[0] as RecognitionResult;
                ObservableCollection<RecognizedWordUnit> words = new ObservableCollection<RecognizedWordUnit>(r.Words);
                ObservableCollection<RecognizedPhrase> alternates = new ObservableCollection<RecognizedPhrase>(r.Alternates);
                
                dataGrid.ItemsSource = alternates;
            }
        }

    }
}
