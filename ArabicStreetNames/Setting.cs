using System.Collections.Generic;
using Colossal;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;

namespace ArabicStreetNames
{
    /// <summary>The mod's page in the game's Options.</summary>
    [FileLocation(nameof(ArabicStreetNames))]
    [SettingsUIGroupOrder(kLabelsGroup, kFontGroup, kAboutGroup)]
    [SettingsUIShowGroupName(kLabelsGroup, kFontGroup, kAboutGroup)]
    public sealed class Setting : ModSetting
    {
        public const string kSection = "Main";
        public const string kLabelsGroup = "Labels";
        public const string kFontGroup = "Font";
        public const string kAboutGroup = "About";

        /// <summary>Font used for the Arabic letter forms when the game's own label fonts do not have them.</summary>
        public enum FallbackFont
        {
            Auto,
            Arial,
            Tahoma,
            SegoeUI,
            None,
        }

        public Setting(IMod mod) : base(mod)
        {
            SetDefaults();
        }

        /// <summary>Shape and reorder Arabic road and district names on the map.</summary>
        [SettingsUISection(kSection, kLabelsGroup)]
        public bool Enabled { get; set; }

        /// <summary>Keep harakat (fatha, damma, kasra...). Off by default: the label renderer cannot place them over letters.</summary>
        [SettingsUISection(kSection, kLabelsGroup)]
        public bool KeepDiacritics { get; set; }

        /// <summary>Extra space between words of an Arabic name, in percent of the letter size.</summary>
        [SettingsUISlider(min = 0, max = 100, step = 5, unit = "percentage")]
        [SettingsUISection(kSection, kLabelsGroup)]
        public int WordSpacing { get; set; }

        /// <summary>Size of Arabic letters relative to the label (100 = as the game draws them).</summary>
        [SettingsUISlider(min = 60, max = 140, step = 5, unit = "percentage")]
        [SettingsUISection(kSection, kLabelsGroup)]
        public int ArabicSize { get; set; }

        [SettingsUISection(kSection, kFontGroup)]
        public FallbackFont Font { get; set; }

        /// <summary>What the font check found (shown on the Options page).</summary>
        [SettingsUISection(kSection, kFontGroup)]
        public string FontStatus => Rendering.LabelFont.StatusText;

        [SettingsUISection(kSection, kAboutGroup)]
        public string Version => typeof(Setting).Assembly.GetName().Version.ToString(3);

        public override void SetDefaults()
        {
            Enabled = true;
            KeepDiacritics = false;
            // Tested in game: the fallback Arabic glyphs are drawn much larger than their advance at 100%, so letters
            // run into each other; 60% sits them at the size of the game's other labels with a natural word gap.
            WordSpacing = 10;
            ArabicSize = 60;
            Font = FallbackFont.Auto;
        }

        public override void Apply()
        {
            base.Apply();
            // Names drawn with the old options are redrawn.
            Patches.LabelNamePatch.RefreshLabels();
        }
    }

    /// <summary>The Options page in Arabic (served for every game language: the game has no Arabic locale).</summary>
    public sealed class LocaleAR : IDictionarySource
    {
        private readonly Setting m_Setting;

        public LocaleAR(Setting setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), "أسماء الشوارع العربية" },
                { m_Setting.GetOptionTabLocaleID(Setting.kSection), "الرئيسية" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kLabelsGroup), "أسماء الخريطة" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kFontGroup), "الخط" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup), "حول" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Enabled)), "تفعيل المود (الأسماء العربية على الخريطة)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Enabled)),
                    "يرسم أسماء الطرق والأحياء العربية على الخريطة بحروف متصلة ومن اليمين إلى اليسار. إيقافه يرجّع الخريطة فوراً كما ترسمها اللعبة بدون المود (مفيد لصور قبل وبعد)، وتشغيله يرجّع الإصلاح فوراً، بدون إعادة تشغيل اللعبة. الأسماء في القوائم لا تتأثر." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.KeepDiacritics)), "إبقاء التشكيل (الحركات)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.KeepDiacritics)),
                    "يبقي الفتحة والضمة والكسرة وغيرها. محرك أسماء الخريطة لا يضعها فوق الحروف، لذلك تُحذف افتراضياً." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.WordSpacing)), "المسافة بين الكلمات" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.WordSpacing)),
                    "مسافة إضافية بين كلمات الاسم فقط (لا تغيّر المسافة بين الحروف، فالحروف العربية متصلة). نسبة من حجم الحرف. الافتراضي 10%." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ArabicSize)), "حجم الحروف العربية" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ArabicSize)),
                    "إذا تداخلت الحروف داخل الكلمة فصغّر هذا الحجم: اللعبة ترسم الحروف العربية أكبر من مساحتها. الافتراضي 60% يطابق حجم بقية أسماء الخريطة." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Font)), "الخط الاحتياطي" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Font)),
                    "يُستخدم لأشكال الحروف المتصلة إذا لم تكن في خطوط اللعبة. تلقائي: يجرب Arial ثم Tahoma ثم Segoe UI. يطبق بعد إعادة تشغيل اللعبة إذا أُضيف خط من قبل." },
                { m_Setting.GetEnumValueLocaleID(Setting.FallbackFont.Auto), "تلقائي" },
                { m_Setting.GetEnumValueLocaleID(Setting.FallbackFont.Arial), "Arial" },
                { m_Setting.GetEnumValueLocaleID(Setting.FallbackFont.Tahoma), "Tahoma" },
                { m_Setting.GetEnumValueLocaleID(Setting.FallbackFont.SegoeUI), "Segoe UI" },
                { m_Setting.GetEnumValueLocaleID(Setting.FallbackFont.None), "بدون (خطوط اللعبة فقط)" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.FontStatus)), "حالة الخط" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.FontStatus)), "نتيجة فحص الخط عند أول رسم لأسماء الخريطة." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Version)), "الإصدار" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Version)), "الإصدار المثبت من المود." },
            };
        }

        public void Unload()
        {
        }
    }
}
