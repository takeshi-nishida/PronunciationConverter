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
//using System.Speech.Synthesis;
using System.Speech.Recognition;
using Microsoft.Speech.Synthesis;
//using Microsoft.Speech.Recognition;
using System.Media;
using Microsoft.Win32;

namespace PronunciationConverter
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        private SpeechSynthesizer synthesizer;
        private SpeechRecognitionEngine recognizer;

        public MainWindow()
        {
            InitializeComponent();

            synthesizer = new SpeechSynthesizer();

            foreach (InstalledVoice voice in synthesizer.GetInstalledVoices())
            {
                selectVoice.Items.Add(voice.VoiceInfo.Name);
            }
            selectVoice.SelectedIndex = 0;
        }

        ///////////////////////////////////////////////////////////////////////
        // Input
        ///////////////////////////////////////////////////////////////////////

        private void play()
        {
            if (inputFilePath.Text.EndsWith(".wav"))
            {
                SoundPlayer player = new SoundPlayer(inputFilePath.Text);
                player.Play();
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
                recognized.Text = "";
            }
        }

        private void startRecognizing()
        {
            initRecognizer();

            recognizer.SetInputToDefaultAudioDevice();
            recognizer.RecognizeAsync(RecognizeMode.Multiple);
            recognized.Text = "";
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
        }


        void sre_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            recognized.Text += e.Result.Text + ". ";
        }

        ///////////////////////////////////////////////////////////////////////
        // Speak
        ///////////////////////////////////////////////////////////////////////

        private void speak()
        {
            synthesizer.Volume = 100;
//            synthesizer.Rate = -2;
            synthesizer.SelectVoice(selectVoice.SelectedItem.ToString());

            SoundPlayer player = new SoundPlayer();
            player.Stream = new System.IO.MemoryStream();
            synthesizer.SetOutputToWaveStream(player.Stream);

            synthesizer.Speak(recognized.Text);
            player.Stream.Position = 0;
            player.Play();
        }

        private void speakToFile()
        {
            MessageBox.Show("Speak to file is not implemented yet.");
        }

        ///////////////////////////////////////////////////////////////////////
        // UI event handlers
        ///////////////////////////////////////////////////////////////////////

        private void chooseFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.FileName = "";
            dialog.Filter = "wav files|*.wav";

            if (dialog.ShowDialog() == true)
            {
                inputFilePath.Text = dialog.FileName;
            }
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            play();
        }


        private void recognizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (inputFromFile.IsChecked == true)
            {
                recognizeFile();
            }
            else if (inputFromMicrophone.IsChecked == true)
            {
                startRecognizing();
            }
        }

        private void speakButton_Click(object sender, RoutedEventArgs e)
        {
            speak();
        }

        private void speakToFileButton_Click(object sender, RoutedEventArgs e)
        {
            speakToFile();
        }

    }
}
