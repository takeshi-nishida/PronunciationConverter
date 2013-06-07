using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Speech.Recognition;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Speech.Synthesis;
using Microsoft.Win32;
using System.Collections.Generic;

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
        private ObservableCollection<SettingSnapshot> settings;

        public MainWindow()
        {
            InitializeComponent();

            ngWords = global::PronunciationConverter2.Properties.Resources.UsePhoneme.Split(',');

            results = new ObservableCollection<RecognitionResult>();
            historyList.ItemsSource = results;
            initSettings();

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
            if (realtimeMode.IsChecked == true)
            {
                speakRealtime(e.Result);
            }
        }

        ///////////////////////////////////////////////////////////////////////
        // Speak
        ///////////////////////////////////////////////////////////////////////

               
        private void speakRealtime(RecognitionResult r)
        {
            initSynthesizer(false);
            feedSynthesizer(r);
            player.Stream.Position = 0;
            player.Play();
        }

        private void speak(bool toFile)
        {
            if (results.Count > 0)
            {
                initSynthesizer(toFile);
                foreach (RecognitionResult r in results) feedSynthesizer(r);
                if (toFile)
                {
                    synthesizer.SetOutputToNull();
                    MessageBox.Show("Conversion finished.");
                }
                else
                {
                    player.Stream.Position = 0;
                    player.Play();
                }

            }
        }

        private void initSynthesizer(bool toFile)
        {
            synthesizer.Volume = 100;
            synthesizer.Rate = (int)rateSlider.Value;
            synthesizer.SelectVoice((selectVoice.SelectedItem as VoiceInfo).Name);

            if (toFile)
            {
                String fname = System.IO.Path.Combine(outputFilePath.Text, System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".wav");
                synthesizer.SetOutputToWaveFile(fname);
            }
            else
            {
                player = new SoundPlayer();
                player.Stream = new System.IO.MemoryStream();
                synthesizer.SetOutputToWaveStream(player.Stream);
            }
        }

        private void feedSynthesizer(RecognitionResult r)
        {
            if (usePhoneme.IsChecked == true)
                synthesizer.Speak(buildPromptBuilder(r));
            else
                synthesizer.Speak(r.Text);
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
                pb.AppendTextWithPronunciation(w.Text, Japanizer.japanize(w.Pronunciation, w.Text));
            }
            pb.AppendSsmlMarkup("</voice>");
            return pb;
        }

        //private String buildPhonemeElement(RecognizedWordUnit w)
        //{
        //    return String.Format("<phoneme alphabet='ipa' ph='{0}'> {1} </phoneme>", w.Pronunciation, w.Text);
        //}

        ///////////////////////////////////////////////////////////////////////
        // Setting
        ///////////////////////////////////////////////////////////////////////

        private void initSettings()
        {
            settings = new ObservableCollection<SettingSnapshot>();
            settingBox.ItemsSource = settings;
            List<SettingSnapshot> ls = SettingSnapshot.loadSettingSnapshots();
            ls.ForEach(settings.Add);
        }

        private void restoreSetting(SettingSnapshot s)
        {
            inputFromFile.IsChecked = s.inputFromFile;
            inputFromMicrophone.IsChecked = s.inputFromMicrophone;
            inputFilePath.Text = s.inputFilePath;
            outputToSpeaker.IsChecked = s.outputToSpeaker;
            outputToFile.IsChecked = s.outputToFile;
            outputFilePath.Text = s.outputFilePath;
            usePhoneme.IsChecked = s.usePhoneme;
            selectVoice.SelectedItem = synthesizer.GetInstalledVoices().First(v => v.VoiceInfo.Name == s.voiceName).VoiceInfo;
            rateSlider.Value = s.speakSpead;
        }

        private void saveSettings()
        {
            SettingSnapshot s = new SettingSnapshot(){
                createdAt = System.DateTime.Now,
                inputFromFile = inputFromFile.IsChecked == true,
                inputFromMicrophone = inputFromMicrophone.IsChecked == true,
                inputFilePath = inputFilePath.Text,
                outputToFile = outputToFile.IsChecked == true,
                outputToSpeaker = outputToSpeaker.IsChecked == true,
                outputFilePath = outputFilePath.Text,
                usePhoneme = usePhoneme.IsChecked == true,
                voiceName = (selectVoice.SelectedItem as VoiceInfo).Name,
                speakSpead = (int) rateSlider.Value
            };
            s.save();
            settings.Add(s);
        }

        ///////////////////////////////////////////////////////////////////////
        // UI event handlers
        ///////////////////////////////////////////////////////////////////////

        private void selectFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.FileName = "";
            dialog.Filter = "wav files|*.wav";
            

            if (dialog.ShowDialog().Equals(true))
            {
                inputFilePath.Text = dialog.FileName;
            }
        }

        private void selectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            outputFilePath.Text = FolderSelector.select();
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            play();
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (inputFromFile.IsChecked == true) recognizeFile();
            else if(inputFromMicrophone.IsChecked == true) startRecognizing();
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            stop();
        }

        private void speakButton_Click(object sender, RoutedEventArgs e)
        {
            if (outputToFile.IsChecked == true) speak(true);
            else if (outputToSpeaker.IsChecked == true) speak(false);
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

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                RecognizedPhrase phrase = e.AddedItems[0] as RecognizedPhrase;
                String text = String.Join(" ", phrase.Words.Select(w => w.Pronunciation)) + "," + String.Join(" ", phrase.Words.Select(w => Japanizer.japanize(w.Pronunciation, w.Text)));
                Clipboard.SetText(text);
            }
        }

        private void saveSettingButton_Click(object sender, RoutedEventArgs e)
        {
            saveSettings();
        }

        private void settingBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                SettingSnapshot s = e.AddedItems[0] as SettingSnapshot;
                restoreSetting(s);
            }
        }
    }
}
