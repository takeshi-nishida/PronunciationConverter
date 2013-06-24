using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Media;
using System.Speech.Recognition;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Speech.Synthesis;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Threading;

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
            resultsList.ItemsSource = results;

            synthesizer = new SpeechSynthesizer();
            player = new SoundPlayer();

            foreach (InstalledVoice voice in synthesizer.GetInstalledVoices())
            {
                selectVoice.Items.Add(voice.VoiceInfo);
            }
            selectVoice.SelectedIndex = 0;

            initSettings();
        }

        ///////////////////////////////////////////////////////////////////////
        // Recognize
        ///////////////////////////////////////////////////////////////////////

        private void recognizeFile()
        {
            initRecognizer();
            results.Clear();

            if (inputFilePath.Text.Length > 0)
            {
                recognizer.SetInputToWaveFile(inputFilePath.Text);
                recognizer.RecognizeAsync(RecognizeMode.Multiple);
            }
        }

        private void recognizeOnce()
        {
            initRecognizer();

            recognizer.SetInputToDefaultAudioDevice();
            RecognitionResult r = recognizer.Recognize();
//            results.Add(r);
            speakResults(new List<RecognitionResult> { r });
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
            results.Add(e.Result);
        }

        ///////////////////////////////////////////////////////////////////////
        // Synthesis
        ///////////////////////////////////////////////////////////////////////

        private void speakResults(List<RecognitionResult> rs)
        {
            if (results.Count > 0)
            {
                synthesizer.Volume = 100;
                synthesizer.Rate = (int)rateSlider.Value;
                synthesizer.SelectVoice((selectVoice.SelectedItem as VoiceInfo).Name);
                player.Stream = new MemoryStream();
                synthesizer.SetOutputToWaveStream(player.Stream);

                feedSynthesizer(rs);

                player.Stream.Position = 0;
                player.Play();
            }
        }

        private void saveConverted(string dirPath)
        {
            if (results.Count > 0)
            {
                synthesizer.Volume = 100;
                synthesizer.Rate = (int)rateSlider.Value;
                synthesizer.SelectVoice((selectVoice.SelectedItem as VoiceInfo).Name);
                synthesizer.SetOutputToWaveFile(Path.Combine(dirPath, "result.wav"));

                feedSynthesizer(results.ToList());

                synthesizer.SetOutputToNull(); // release file lock
                MessageBox.Show("Conversion finished.");
            }
        }

        private void feedSynthesizer(List<RecognitionResult> rs)
        {
            bool b = usePhoneme.IsChecked == true;
            string voiceLang = (selectVoice.SelectedItem as VoiceInfo).Culture.Name;
            foreach (RecognitionResult r in rs)
            {
                if (b) synthesizer.Speak(buildPromptBuilder(r, voiceLang));
                else synthesizer.Speak(r.Text);
            }
        }

        private PromptBuilder buildPromptBuilder(RecognitionResult result, string voiceLang)
        {
            PromptBuilder pb = new PromptBuilder();
            pb.AppendSsmlMarkup(String.Format("<voice xml:lang='{0}'>", voiceLang));
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


        ///////////////////////////////////////////////////////////////////////
        // Original voice (play, stop, and save)
        ///////////////////////////////////////////////////////////////////////

        private CancellationTokenSource canceller = new CancellationTokenSource();

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
                canceller.Cancel();
            }
        }

        private void playOriginal()
        {
            List<RecognizedAudio> lst = new List<RecognizedAudio>();
            foreach (RecognitionResult r in results) lst.Add(r.Audio);

            Task t = Task.Factory.StartNew(() =>
            {
                foreach (RecognizedAudio a in lst)
                {
                    if (canceller.Token.IsCancellationRequested) break;
                    player.Stream = new MemoryStream();
                    a.WriteToWaveStream(player.Stream);
                    player.Stream.Position = 0;
                    player.PlaySync();
                }
            }, canceller.Token);
        }

        private void saveOriginal(string dirPath)
        {
            foreach (RecognitionResult r in results)
            {
                string path = Path.Combine(dirPath, r.Audio.StartTime.ToString("yyyyMMdd_HHmmss") + ".wav");
                using (FileStream s = File.Create(path)) r.Audio.WriteToWaveStream(s);
            }
        }

        ///////////////////////////////////////////////////////////////////////
        // Setting
        ///////////////////////////////////////////////////////////////////////

        private void initSettings()
        {
            settings = new ObservableCollection<SettingSnapshot>();
            settingBox.ItemsSource = settings;
            List<SettingSnapshot> ls = SettingSnapshot.loadSettingSnapshots();
            if (ls.Count > 0)
            {
                ls.ForEach(settings.Add);
                settingBox.SelectedItem = ls.Last();
            }
        }

        private SettingSnapshot snap()
        {
            return new SettingSnapshot()
            {
                createdAt = System.DateTime.Now,
                selectedTabIndex = inputTab.SelectedIndex,
                inputFilePath = inputFilePath.Text,
                outputFolderPath = outputFolderPath.Text,
                usePhoneme = usePhoneme.IsChecked == true,
                voiceName = (selectVoice.SelectedItem as VoiceInfo).Name,
                speakSpead = (int)rateSlider.Value,
            };
        }

        private void restoreSetting(SettingSnapshot s)
        {
            inputFilePath.Text = s.inputFilePath;
            inputTab.SelectedIndex = s.selectedTabIndex;
            outputFolderPath.Text = s.outputFolderPath;
            usePhoneme.IsChecked = s.usePhoneme;
            selectVoice.SelectedItem = synthesizer.GetInstalledVoices().First(v => v.VoiceInfo.Name == s.voiceName).VoiceInfo;
            rateSlider.Value = s.speakSpead;
        }

        private void saveSettings()
        {
            SettingSnapshot s = snap();
            s.save();
            settings.Add(s);
            settingBox.SelectedItem = s;
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
            outputFolderPath.Text = FolderSelector.select(outputFolderPath.Text);
        }

        private void outputFolderPath_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("explorer.exe", string.Format("/root,\"{0}\"", outputFolderPath.Text));
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            play();
        }

        private void stopButton_Click(object sender, RoutedEventArgs e)
        {
            stop();
        }

        private void listenButton_Click(object sender, RoutedEventArgs e)
        {
            listenButton.IsEnabled = false;
            recognizeOnce();
            listenButton.IsEnabled = true;
        }

        private void listenFileButton_Click(object sender, RoutedEventArgs e)
        {
            recognizeFile();
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

        private void playConvertedButton_Click(object sender, RoutedEventArgs e)
        {
            speakResults(results.ToList());
        }

        private void playOriginalButton_Click(object sender, RoutedEventArgs e)
        {
            playOriginal();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            string dirPath = Path.Combine(outputFolderPath.Text, DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

            saveConverted(dirPath);
            saveOriginal(dirPath);
        }
    }
}
