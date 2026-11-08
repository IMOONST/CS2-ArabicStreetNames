using System.Globalization;
using System.Text;
using ArabicStreetNames.Text;

namespace ArabicStreetNames.Patches
{
    /// <summary>
    /// Spacing and size for Arabic in a map label, as TextMeshPro rich-text tags (the label renderer parses them).
    /// The space between words comes from the game's Latin label font and is narrow next to Arabic letters, whose
    /// swashes (ق ع ي ن) reach past their own width, so neighbouring words run into each other: every space gets
    /// extra width. Arabic runs can also be drawn larger or smaller than the Latin letters around them.
    /// </summary>
    public static class LabelLayout
    {
        /// <param name="visual">The label in visual order (already shaped).</param>
        /// <param name="wordSpacingPercent">Extra space between words, in percent of the letter size (0 = none).</param>
        /// <param name="arabicSizePercent">Size of Arabic runs relative to the label (100 = unchanged).</param>
        public static string Apply(string visual, int wordSpacingPercent, int arabicSizePercent)
        {
            if (string.IsNullOrEmpty(visual)) return visual;
            var resize = arabicSizePercent > 0 && arabicSizePercent != 100;
            var gap = wordSpacingPercent > 0
                ? "<space=" + (wordSpacingPercent / 100f).ToString("0.###", CultureInfo.InvariantCulture) + "em>"
                : null;
            if (!resize && gap == null) return visual;

            var size = "<size=" + arabicSizePercent.ToString(CultureInfo.InvariantCulture) + "%>";
            var result = new StringBuilder(visual.Length + 32);
            var inArabic = false;
            for (var i = 0; i < visual.Length; i++)
            {
                var c = visual[i];
                var arabic = ArabicBidi.IsRtl(c);
                if (resize && arabic && !inArabic)
                {
                    result.Append(size);
                    inArabic = true;
                }
                else if (resize && inArabic && !arabic && c != ' ')
                {
                    result.Append("</size>");
                    inArabic = false;
                }
                result.Append(c);
                if (c == ' ' && gap != null) result.Append(gap);
            }
            if (inArabic) result.Append("</size>");
            return result.ToString();
        }
    }
}
