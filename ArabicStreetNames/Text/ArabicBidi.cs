using System.Collections.Generic;
using System.Text;

namespace ArabicStreetNames.Text
{
    /// <summary>
    /// Puts a line of mixed right-to-left and left-to-right text into visual (left-to-right) order for a renderer
    /// that only lays text out left to right. Works at run level, following the Unicode Bidirectional Algorithm
    /// (UAX #9) for single-line labels: weak types (numbers stay left-to-right inside Arabic), neutrals take the
    /// direction of the runs around them, L2 reordering by embedding level, and mirrored brackets in right-to-left
    /// runs. The paragraph direction comes from the first strong character.
    /// </summary>
    public static class ArabicBidi
    {
        private enum Kind { R, L, EN, N }

        private sealed class Run
        {
            public readonly StringBuilder Text = new StringBuilder();
            public Kind Kind;
            public int Level;
        }

        private static readonly Dictionary<char, char> s_Mirror = new Dictionary<char, char>
        {
            ['('] = ')', [')'] = '(', ['['] = ']', [']'] = '[', ['{'] = '}', ['}'] = '{',
            ['<'] = '>', ['>'] = '<', ['«'] = '»', ['»'] = '«',
        };

        public static bool IsRtl(char c) =>
            (c >= '֐' && c <= 'ࣿ') || (c >= 'יִ' && c <= '﷿') || (c >= 'ﹰ' && c <= '﻿');

        private static bool IsDigit(char c) => (c >= '0' && c <= '9') || (c >= '٠' && c <= '٩') || (c >= '۰' && c <= '۹');

        private static Kind KindOf(char c)
        {
            // Arabic-Indic digits sit inside the Arabic block: they are numbers, not letters.
            if (IsDigit(c)) return Kind.EN;
            if (IsRtl(c)) return Kind.R;
            if (char.IsLetter(c)) return Kind.L;
            return Kind.N;
        }

        public static bool ContainsRtl(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            foreach (var c in text) if (IsRtl(c) && !IsDigit(c)) return true;
            return false;
        }

        /// <summary>The first strong character decides: right-to-left when it is Arabic or Hebrew.</summary>
        public static bool IsRtlParagraph(string text)
        {
            foreach (var c in text)
            {
                var k = KindOf(c);
                if (k == Kind.R) return true;
                if (k == Kind.L) return false;
            }
            return false;
        }

        /// <summary>The characters of <paramref name="text"/> (one line) in visual left-to-right order.</summary>
        public static string ToVisual(string text)
        {
            if (!ContainsRtl(text)) return text;

            // 1. Runs of one kind.
            var runs = new List<Run>();
            foreach (var c in text)
            {
                var k = KindOf(c);
                if (runs.Count == 0 || runs[runs.Count - 1].Kind != k) runs.Add(new Run { Kind = k });
                runs[runs.Count - 1].Text.Append(c);
            }
            // Words of one script stay together across single spaces, and "1,250" stays one number.
            for (var j = 1; j < runs.Count - 1; j++)
            {
                Run a = runs[j - 1], n = runs[j], b = runs[j + 1];
                var gap = n.Text.ToString();
                var joinWords = n.Kind == Kind.N && gap.Trim().Length == 0 && a.Kind == b.Kind && (a.Kind == Kind.R || a.Kind == Kind.L);
                var joinNumber = n.Kind == Kind.N && (gap == "." || gap == "," || gap == "٫" || gap == "٬") && a.Kind == Kind.EN && b.Kind == Kind.EN;
                if (joinWords || joinNumber)
                {
                    a.Text.Append(gap).Append(b.Text);
                    runs.RemoveRange(j, 2);
                    j--;
                }
            }

            // 2. Levels: R = 1; L = 2 in a right-to-left paragraph (0 otherwise); numbers right after Arabic, or in
            //    a right-to-left paragraph, are 2 (left-to-right inside right-to-left).
            var baseLevel = IsRtlParagraph(text) ? 1 : 0;
            var lastStrong = baseLevel == 1 ? Kind.R : Kind.L;
            foreach (var r in runs)
            {
                if (r.Kind == Kind.R) { r.Level = 1; lastStrong = Kind.R; }
                else if (r.Kind == Kind.L) { r.Level = baseLevel == 1 ? 2 : 0; lastStrong = Kind.L; }
                else if (r.Kind == Kind.EN) r.Level = baseLevel == 1 || lastStrong == Kind.R ? 2 : 0;
            }
            // Neutrals take the direction of the runs on both sides when they agree, else the paragraph's.
            Kind Side(int index)
            {
                if (index < 0 || index >= runs.Count) return baseLevel == 1 ? Kind.R : Kind.L;
                var r = runs[index];
                if (r.Kind == Kind.EN) return r.Level > 0 ? Kind.R : Kind.L;
                return r.Kind;
            }
            for (var x = 0; x < runs.Count; x++)
            {
                if (runs[x].Kind != Kind.N) continue;
                var p = x - 1;
                while (p >= 0 && runs[p].Kind == Kind.N) p--;
                var q = x + 1;
                while (q < runs.Count && runs[q].Kind == Kind.N) q++;
                Kind before = Side(p), after = Side(q);
                runs[x].Level = before == after ? (before == Kind.R ? 1 : (baseLevel == 1 ? 2 : 0)) : baseLevel;
            }

            // 3. L2: from the highest level down to 1, reverse every sequence of runs at or above that level.
            var max = 0;
            foreach (var r in runs) if (r.Level > max) max = r.Level;
            for (var level = max; level >= 1; level--)
            {
                for (var s = 0; s < runs.Count; s++)
                {
                    if (runs[s].Level < level) continue;
                    var e = s;
                    while (e + 1 < runs.Count && runs[e + 1].Level >= level) e++;
                    runs.Reverse(s, e - s + 1);
                    s = e;
                }
            }

            // 4. Characters: odd-level runs read right to left, so their characters are reversed (brackets mirrored).
            var visual = new StringBuilder(text.Length);
            foreach (var r in runs)
            {
                var t = r.Text.ToString();
                if (r.Level % 2 == 1)
                {
                    for (var i = t.Length - 1; i >= 0; i--)
                    {
                        var c = t[i];
                        visual.Append(s_Mirror.TryGetValue(c, out var m) ? m : c);
                    }
                }
                else visual.Append(t);
            }
            return visual.ToString();
        }
    }
}
