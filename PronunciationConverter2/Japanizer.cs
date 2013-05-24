using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PronunciationConverter2
{
    class Japanizer
    {
        // pliz  juz  ðə fidbæk   fɔɻm  tə ɻikwɛst  ɛni ədɪʃənəl  ɪkspɻɛʃənz  ɔɻ ɪf jʊ hæv  ɛni səd͡ʒɛst͡ʃənz  fə˞ ðə sa͡it
        // plizɯ jɯzɯ zə fidbakkɯ foːmɯ tə ɽikwɛsto ɛni ədɪʃənəlɯ ɪkspɽɛʃənzɯ oː ɪf jɯ habɯ ɛni səd͡ʒɛst͡ʃənzɯ fəː zə sa͡ito
        // θæŋk ju ən gʊd lʌk,saŋkɯ ju ən gɯddo lakkɯ

        private const String oConsonants = "dt";
        private const String uConsonants = "bfgklmpsvz";
        private const String consonants = oConsonants + uConsonants;
        private const String sVowels = "aɪɯ";
        private const String lVowels = "iuɛeo";
        private const String vowels = sVowels + lVowels;

        private static JapanizeRule[] rules =
        {
            // Vowels
            new JapanizeRule("[æɑʌɒ]", "a"),
            //new ReplaceRule("", "i"),
            new JapanizeRule("[ʊ]", "ɯ"),
            //new JapanizeRule("ɛ", "e"),
            new JapanizeRule("ɔ", "o"),

            // Consonants
            new JapanizeRule("v", "b"),
            new JapanizeRule("θ", "s"),
            new JapanizeRule("ð", "z"),
            
            // R
            new JapanizeRule(String.Format("[ɻ]([{0}])", vowels), "ɽ$1"),
            new JapanizeRule("[ɻ˞]", "ː"),

            // Two consonants in a row
            new JapanizeRule(String.Format("([{0}])([{1}])([{2}])", sVowels, uConsonants, consonants), "$1$2$2ɯ$3"),
            new JapanizeRule(String.Format("([{0}])([{1}])([{2}])", lVowels, uConsonants, consonants), "$1$2ɯ$3"),
            new JapanizeRule(String.Format("([{0}])([{1}])([{2}])", sVowels, oConsonants, consonants), "$1$2$2o$3"),
            new JapanizeRule(String.Format("([{0}])([{1}])([{2}])", lVowels, oConsonants, consonants), "$1$2o$3"),

            // Words ending with consonants
            new JapanizeRule(String.Format("([{0}])([{1}])$", sVowels, uConsonants), "$1$2$2ɯ"),
            new JapanizeRule(String.Format("([{0}])([{1}])$", lVowels, uConsonants), "$1$2ɯ"),
            new JapanizeRule(String.Format("([{0}])([{1}])$", sVowels, oConsonants), "$1$2$2o"),
            new JapanizeRule(String.Format("([{0}])([{1}])$", lVowels, oConsonants), "$1$2o"),
            new JapanizeRule(String.Format("([{0}])$", uConsonants), "$1ɯ"),
            new JapanizeRule(String.Format("([{0}])$", oConsonants), "$1o"),

            // Other endings
            new JapanizeRule("ŋ$", "ŋgɯ")
        };

        private static SpellingRule[] srules =
        {
            new SpellingRule("d$", "([^d])$", "$1d"),
            new SpellingRule("to$", "tə$", "tuː"),
            new SpellingRule("or$", "[əɔ][ɻ˞]$", "ɔaː"),
        };

        public static String japanize(String pronunciation, String spelling)
        {
            foreach (SpellingRule rule in srules) pronunciation = rule.applyTo(pronunciation, spelling);
            foreach (JapanizeRule rule in rules) pronunciation = rule.applyTo(pronunciation);
            return pronunciation;
        }
    }

    class JapanizeRule
    {
        private Regex r;
        private String to;

        public JapanizeRule(String pattern, String to)
        {
            r = new Regex(pattern);
            this.to = to;
        }

        public String applyTo(String s)
        {
            return r.Replace(s, to);
        }
    }

    class SpellingRule
    {
        private Regex rs, rp;
        private String to;

        public SpellingRule(String ps, String pp, String to)
        {
            rs = new Regex(ps);
            rp = new Regex(pp);
            this.to = to;
        }

        public String applyTo(String pronunciation, String spelling)
        {
            return rs.IsMatch(spelling) ? rp.Replace(pronunciation, to) : pronunciation;
        }
    }
}
