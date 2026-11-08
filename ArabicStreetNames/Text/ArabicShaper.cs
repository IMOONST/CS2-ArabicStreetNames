using System.Collections.Generic;
using System.Text;

namespace ArabicStreetNames.Text
{
    /// <summary>
    /// Contextual shaping for Arabic script: replaces each letter with its presentation form (isolated, final,
    /// initial or medial, from the Arabic Presentation Forms-A/B blocks) according to its neighbours, and joins
    /// lam + alef into the mandatory ligatures. Text renderers without an OpenType shaping engine (the game's world
    /// labels use TextMeshPro) then draw connected letters. The result is still in logical order; see
    /// <see cref="ArabicBidi"/> for the visual order.
    /// </summary>
    public static class ArabicShaper
    {
        /// <summary>Forms of one letter: isolated, final, initial, medial (0 = the letter has no such form).</summary>
        private struct Forms
        {
            public char Isolated, Final, Initial, Medial;
            public Forms(int isolated, int final, int initial = 0, int medial = 0)
            {
                Isolated = (char)isolated;
                Final = (char)final;
                Initial = (char)initial;
                Medial = (char)medial;
            }

            /// <summary>Dual-joining letters connect on both sides; right-joining ones only to the letter before.</summary>
            public bool JoinsForward => Initial != 0;
        }

        private static readonly Dictionary<char, Forms> s_Forms = new Dictionary<char, Forms>
        {
            ['ء'] = new Forms(0xFE80, 0xFE80),                 // ء (does not join)
            ['آ'] = new Forms(0xFE81, 0xFE82),                 // آ
            ['أ'] = new Forms(0xFE83, 0xFE84),                 // أ
            ['ؤ'] = new Forms(0xFE85, 0xFE86),                 // ؤ
            ['إ'] = new Forms(0xFE87, 0xFE88),                 // إ
            ['ئ'] = new Forms(0xFE89, 0xFE8A, 0xFE8B, 0xFE8C), // ئ
            ['ا'] = new Forms(0xFE8D, 0xFE8E),                 // ا
            ['ب'] = new Forms(0xFE8F, 0xFE90, 0xFE91, 0xFE92), // ب
            ['ة'] = new Forms(0xFE93, 0xFE94),                 // ة
            ['ت'] = new Forms(0xFE95, 0xFE96, 0xFE97, 0xFE98), // ت
            ['ث'] = new Forms(0xFE99, 0xFE9A, 0xFE9B, 0xFE9C), // ث
            ['ج'] = new Forms(0xFE9D, 0xFE9E, 0xFE9F, 0xFEA0), // ج
            ['ح'] = new Forms(0xFEA1, 0xFEA2, 0xFEA3, 0xFEA4), // ح
            ['خ'] = new Forms(0xFEA5, 0xFEA6, 0xFEA7, 0xFEA8), // خ
            ['د'] = new Forms(0xFEA9, 0xFEAA),                 // د
            ['ذ'] = new Forms(0xFEAB, 0xFEAC),                 // ذ
            ['ر'] = new Forms(0xFEAD, 0xFEAE),                 // ر
            ['ز'] = new Forms(0xFEAF, 0xFEB0),                 // ز
            ['س'] = new Forms(0xFEB1, 0xFEB2, 0xFEB3, 0xFEB4), // س
            ['ش'] = new Forms(0xFEB5, 0xFEB6, 0xFEB7, 0xFEB8), // ش
            ['ص'] = new Forms(0xFEB9, 0xFEBA, 0xFEBB, 0xFEBC), // ص
            ['ض'] = new Forms(0xFEBD, 0xFEBE, 0xFEBF, 0xFEC0), // ض
            ['ط'] = new Forms(0xFEC1, 0xFEC2, 0xFEC3, 0xFEC4), // ط
            ['ظ'] = new Forms(0xFEC5, 0xFEC6, 0xFEC7, 0xFEC8), // ظ
            ['ع'] = new Forms(0xFEC9, 0xFECA, 0xFECB, 0xFECC), // ع
            ['غ'] = new Forms(0xFECD, 0xFECE, 0xFECF, 0xFED0), // غ
            ['ـ'] = new Forms(0x0640, 0x0640, 0x0640, 0x0640), // ـ tatweel
            ['ف'] = new Forms(0xFED1, 0xFED2, 0xFED3, 0xFED4), // ف
            ['ق'] = new Forms(0xFED5, 0xFED6, 0xFED7, 0xFED8), // ق
            ['ك'] = new Forms(0xFED9, 0xFEDA, 0xFEDB, 0xFEDC), // ك
            ['ل'] = new Forms(0xFEDD, 0xFEDE, 0xFEDF, 0xFEE0), // ل
            ['م'] = new Forms(0xFEE1, 0xFEE2, 0xFEE3, 0xFEE4), // م
            ['ن'] = new Forms(0xFEE5, 0xFEE6, 0xFEE7, 0xFEE8), // ن
            ['ه'] = new Forms(0xFEE9, 0xFEEA, 0xFEEB, 0xFEEC), // ه
            ['و'] = new Forms(0xFEED, 0xFEEE),                 // و
            ['ى'] = new Forms(0xFEEF, 0xFEF0),                 // ى
            ['ي'] = new Forms(0xFEF1, 0xFEF2, 0xFEF3, 0xFEF4), // ي
            ['ٱ'] = new Forms(0xFB50, 0xFB51),                 // ٱ
            ['پ'] = new Forms(0xFB56, 0xFB57, 0xFB58, 0xFB59), // پ
            ['چ'] = new Forms(0xFB7A, 0xFB7B, 0xFB7C, 0xFB7D), // چ
            ['ژ'] = new Forms(0xFB8A, 0xFB8B),                 // ژ
            ['ڤ'] = new Forms(0xFB6A, 0xFB6B, 0xFB6C, 0xFB6D), // ڤ
            ['ک'] = new Forms(0xFB8E, 0xFB8F, 0xFB90, 0xFB91), // ک
            ['گ'] = new Forms(0xFB92, 0xFB93, 0xFB94, 0xFB95), // گ
            ['ی'] = new Forms(0xFBFC, 0xFBFD, 0xFBFE, 0xFBFF), // ی
        };

        /// <summary>Lam followed by these alefs becomes one ligature: (isolated, final).</summary>
        private static readonly Dictionary<char, (char Isolated, char Final)> s_LamAlef = new Dictionary<char, (char, char)>
        {
            ['آ'] = ('ﻵ', 'ﻶ'),
            ['أ'] = ('ﻷ', 'ﻸ'),
            ['إ'] = ('ﻹ', 'ﻺ'),
            ['ا'] = ('ﻻ', 'ﻼ'),
        };

        private const char Lam = 'ل';

        /// <summary>Harakat and other combining marks: transparent to joining.</summary>
        public static bool IsMark(char c) =>
            (c >= 'ً' && c <= 'ٟ') || c == 'ٰ' || (c >= 'ۖ' && c <= 'ۭ') || (c >= 'ؐ' && c <= 'ؚ');

        /// <summary>True when the text contains a letter this shaper changes.</summary>
        public static bool NeedsShaping(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (var c in text) if (s_Forms.ContainsKey(c)) return true;
            return false;
        }

        /// <summary>
        /// Shapes <paramref name="text"/> (logical order in, logical order out). Combining marks are dropped when
        /// <paramref name="keepMarks"/> is false: without an OpenType engine they cannot be positioned over the
        /// presentation forms and would draw as separate characters.
        /// </summary>
        public static string Shape(string text, bool keepMarks = false)
        {
            if (!NeedsShaping(text)) return text;

            // Letters with their marks; joining looks through marks.
            var letters = new List<char>(text.Length);
            var marks = new List<string>(text.Length);
            foreach (var c in text)
            {
                if (IsMark(c))
                {
                    if (keepMarks && letters.Count > 0) marks[marks.Count - 1] += c;
                    continue;
                }
                letters.Add(c);
                marks.Add("");
            }

            var result = new StringBuilder(letters.Count);
            for (var i = 0; i < letters.Count; i++)
            {
                var c = letters[i];
                if (!s_Forms.TryGetValue(c, out var forms))
                {
                    result.Append(c).Append(marks[i]);
                    continue;
                }

                var joinsBack = i > 0 && s_Forms.TryGetValue(letters[i - 1], out var before) && before.JoinsForward;

                // Lam + alef: one mandatory ligature.
                if (c == Lam && i + 1 < letters.Count && s_LamAlef.TryGetValue(letters[i + 1], out var ligature))
                {
                    result.Append(joinsBack ? ligature.Final : ligature.Isolated).Append(marks[i]).Append(marks[i + 1]);
                    i++;
                    continue;
                }

                var joinsForward = forms.JoinsForward && i + 1 < letters.Count && s_Forms.ContainsKey(letters[i + 1]);
                char shaped;
                if (joinsBack && joinsForward) shaped = forms.Medial;
                else if (joinsBack) shaped = forms.Final;
                else if (joinsForward) shaped = forms.Initial;
                else shaped = forms.Isolated;
                result.Append(shaped == 0 ? forms.Isolated : shaped).Append(marks[i]);
            }
            return result.ToString();
        }
    }
}
