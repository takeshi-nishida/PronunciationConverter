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
        public bool usePhoneme { get; set; }
        public string voiceName { get; set; }
        public int speakSpead { get; set; }


        public static List<SettingSnapshot> loadSettingSnapshots()
        {
            List<SettingSnapshot> ls = new List<SettingSnapshot>();
            foreach (string fname in Directory.GetFiles(getExecutingPath(), "*.xml"))
            {
                XmlSerializer reader = new XmlSerializer(typeof(SettingSnapshot));
                StreamReader file = new StreamReader(fname);
                ls.Add((SettingSnapshot)reader.Deserialize(file));
            }
            return ls;
        }

        public SettingSnapshot() { }

        public void save()
        {
            XmlSerializer writer = new XmlSerializer(typeof(SettingSnapshot));
            string fname = Path.Combine(getExecutingPath(), createdAt.ToString("yyyyMMddHHmmss") + ".xml");
            StreamWriter file = new StreamWriter(fname);
            writer.Serialize(file, this);
            file.Close();
        }

        private static string getExecutingPath()
        {
            return Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
        }

        private static Regex regex = new Regex(@"\(.*,");

        public string name
        {
            get
            {
                string lang = regex.Match(voiceName).Value.Substring(1);
                return String.Format("{0} speed:{1}", lang, speakSpead);
            }
        }
    }
}
