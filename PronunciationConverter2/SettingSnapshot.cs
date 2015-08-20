using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using System.Text.RegularExpressions;

namespace PronunciationConverter2
{
    public class SettingSnapshot
    {
        public DateTime createdAt { get; set; }
        public int selectedTabIndex { get; set; }
        public string inputFilePath { get; set; }
        public string outputFolderPath { get; set; }
        public bool useJapanizer { get; set; }
        public bool wordByWord { get; set; }
        public string inputCulture { get; set; }
        public string outputCulture { get; set; }
        public int speakSpead { get; set; }
        public string scenarioName { get; set; }


        public static List<SettingSnapshot> loadSettingSnapshots()
        {
            List<SettingSnapshot> ls = new List<SettingSnapshot>();
            try
            {
                foreach (string fname in Directory.GetFiles(MainWindow.getExecutingPath("settings"), "*.xml"))
                {
                    XmlSerializer reader = new XmlSerializer(typeof(SettingSnapshot));
                    using (StreamReader file = new StreamReader(fname))
                    {
                        ls.Add((SettingSnapshot)reader.Deserialize(file));
                    }
                }
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }
            return ls;
        }

        public SettingSnapshot() { }

        public void save()
        {
            XmlSerializer writer = new XmlSerializer(typeof(SettingSnapshot));
            Directory.CreateDirectory(MainWindow.getExecutingPath("settings"));
            string fname = Path.Combine(MainWindow.getExecutingPath("settings"), createdAt.ToString("yyyyMMddHHmmss") + ".xml");
            using (StreamWriter file = new StreamWriter(fname))
            {
                writer.Serialize(file, this);
            }
        }


        private static Regex regex = new Regex(@"\(.*,");

        public string name
        {
            get
            {
//                string lang = regex.Match(outputCulture).Value;
                return String.Format("{0} → {1} speed:{2}", inputCulture, outputCulture, speakSpead);
            }
        }
    }
}
