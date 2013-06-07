﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace PronunciationConverter2
{
    public class SettingSnapshot
    {
        public DateTime createdAt { get; set; }
        public bool inputFromMicrophone { get; set; }
        public bool inputFromFile { get; set; }
        public string inputFilePath { get; set; }
        public bool outputToSpeaker { get; set; }
        public bool outputToFile { get; set; }
        public string outputFilePath { get; set; }
        public bool usePhoneme { get; set; }
        public string voiceName { get; set; }
        public int speakSpead { get; set; }


        public static List<SettingSnapshot> loadSettingSnapshots()
        {
            List<SettingSnapshot> ls = new List<SettingSnapshot>();
            foreach(string fname in Directory.GetFiles(getExecutingPath(), "*.xml"))
            {
                XmlSerializer reader = new XmlSerializer(typeof(SettingSnapshot));
                StreamReader file = new StreamReader(fname);
                ls.Add((SettingSnapshot) reader.Deserialize(file));
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
    }
}