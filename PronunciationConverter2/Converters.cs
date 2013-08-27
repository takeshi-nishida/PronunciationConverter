using System;
using System.Linq;
using System.Windows.Data;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Speech.Recognition;

namespace PronunciationConverter2
{
    public class EndsWithWavConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((string) value).EndsWith(".wav");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }


    public class GreaterThanZeroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((int) value) > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }

    public class JapanizeWordsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "";
            ReadOnlyCollection<RecognizedWordUnit> words = value as ReadOnlyCollection<RecognizedWordUnit>;
            return String.Join(" ", words.Select(w => w.Pronunciation)) + Environment.NewLine + "→ " + String.Join(" ", words.Select(w => Japanizer.japanize(w.Pronunciation, w.Text)));
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
