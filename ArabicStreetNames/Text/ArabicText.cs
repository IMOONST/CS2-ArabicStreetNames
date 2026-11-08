using System;
using System.Collections.Generic;

namespace ArabicStreetNames.Text
{
    /// <summary>
    /// Arabic text for renderers without shaping or bidi support: shaped presentation forms in visual left-to-right
    /// order. Results are cached (labels are rebuilt often with the same names).
    /// </summary>
    public static class ArabicText
    {
        private const int kCacheLimit = 8192;
        private static readonly Dictionary<string, string> s_Cache = new Dictionary<string, string>(StringComparer.Ordinal);
        private static readonly object s_Lock = new object();

        /// <summary>
        /// <paramref name="shape"/>: use presentation forms (needs a font that has them); without it the letters are
        /// only put in reading order. Lines (\n) are processed separately.
        /// </summary>
        public static string ForLeftToRightRenderer(string text, bool shape, bool keepMarks)
        {
            if (string.IsNullOrEmpty(text) || !ArabicBidi.ContainsRtl(text)) return text;
            var key = (shape ? "S" : "-") + (keepMarks ? "M" : "-") + text;
            lock (s_Lock)
            {
                if (s_Cache.TryGetValue(key, out var cached)) return cached;
            }
            var lines = text.Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (!ArabicBidi.ContainsRtl(line)) continue;
                if (shape) line = ArabicShaper.Shape(line, keepMarks);
                else if (!keepMarks) line = StripMarks(line);
                lines[i] = ArabicBidi.ToVisual(line);
            }
            var result = string.Join("\n", lines);
            lock (s_Lock)
            {
                if (s_Cache.Count >= kCacheLimit) s_Cache.Clear();
                s_Cache[key] = result;
            }
            return result;
        }

        public static void ClearCache()
        {
            lock (s_Lock) s_Cache.Clear();
        }

        private static string StripMarks(string text)
        {
            var chars = new List<char>(text.Length);
            foreach (var c in text) if (!ArabicShaper.IsMark(c)) chars.Add(c);
            return new string(chars.ToArray());
        }
    }
}
