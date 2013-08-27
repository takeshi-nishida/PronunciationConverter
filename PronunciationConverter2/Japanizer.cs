using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PronunciationConverter2
{
    class Japanizer
    {
        private const String oConsonants = "dt";
        private const String uConsonants = "bfgklmpsvzɽ";
        private const String consonants = oConsonants + uConsonants;
        private const String sVowels = "ʌɪʊ";
        private const String lVowels = "aiuɯɛeoː";
        private const String vowels = sVowels + lVowels;

        private static JapanizeRule[] rules =
        {
            // R
            new JapanizeRule("([iɪɛeʊuɯ])ɻ", "$1a"),
            new JapanizeRule("([æɑɒɜaoɔ])ɻ$", "$1a"),
            new JapanizeRule("([æɑɒɜaoɔ])ɻ", "$1ː"),

            // Vowels
            new JapanizeRule("[ɑɒɜə]", "a"), // æ
            //new JapanizeRule("ɪ", "iː"),
            new JapanizeRule("i", "i"),
            //new JapanizeRule("[ʊ]", "uː"),//"ɯ"),
            new JapanizeRule("[uɯ]", "ɯ"),
            new JapanizeRule("ɛ", "e"),
            new JapanizeRule("[oɔ]", "o"),

            // R
            new JapanizeRule(String.Format("ɻ([{0}])", vowels), "l$1"),

            // Consonants
            new JapanizeRule("v", "b"),
            new JapanizeRule("θ", "s"),
            new JapanizeRule("ð", "z"),
            new JapanizeRule("f", "ɸ"), // below was added 2013/8/26
            new JapanizeRule("ʧ", "ʨ"), 
            new JapanizeRule("ʍ", "w"), 
            new JapanizeRule("s[iɪ]", "ʃi"),
            new JapanizeRule("t[iɪ]", "ʧi"),
            new JapanizeRule("t[uɯʊ]", "ʦɯ"),

            // Conconant clusters at word beginning
            new JapanizeRule(String.Format("^([{0}])([{1}])", uConsonants, consonants), "$1u$2"),
            new JapanizeRule(String.Format("^([{0}])([{1}])", oConsonants, consonants), "$1o$2"),

            // Consonant clusters in the middle of words
            new JapanizeRule(String.Format("([{0}])([{1}])([{2}])", sVowels, uConsonants, consonants), "$1$2$2ɯ$3"),
            new JapanizeRule(String.Format("([{0}])([{1}])([{2}])", lVowels, uConsonants, consonants), "$1$2ɯ$3"),
            new JapanizeRule(String.Format("([{0}])([{1}])([{2}])", sVowels, oConsonants, consonants), "$1$2$2o$3"),
            new JapanizeRule(String.Format("([{0}])([{1}])([{2}])", lVowels, oConsonants, consonants), "$1$2o$3"),
            //new JapanizeRule(String.Format("([{0}])([{1}])", uConsonants, consonants), "$1ɯ$2"),
            //new JapanizeRule(String.Format("([{0}])([{1}])", oConsonants, consonants), "$1o$2"),

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
            new SpellingRule("^the$", "^.*$", "za"),    // example: "the"
            new SpellingRule("d$", "([^d])$", "$1d"),   // example: "and", "end"
            new SpellingRule("to$", "tə$", "tuː"),      // example: "to"
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
