using System.Collections.Generic;
using Colossal;
using Game.SceneFlow;
using Colossal.IO.AssetDatabase;
using Game.Modding;
using Game.Settings;

namespace ArabicStreetNames
{
    /// <summary>The mod's page in the game's Options.</summary>
    [FileLocation(nameof(ArabicStreetNames))]
    [SettingsUIGroupOrder(kLabelsGroup, kFontGroup, kLanguageGroup, kAboutGroup)]
    [SettingsUIShowGroupName(kLabelsGroup, kFontGroup, kLanguageGroup, kAboutGroup)]
    public sealed class Setting : ModSetting
    {
        public const string kSection = "Main";
        public const string kLabelsGroup = "Labels";
        public const string kFontGroup = "Font";
        public const string kAboutGroup = "About";
        public const string kLanguageGroup = "Language";

        /// <summary>The language of this page.</summary>
        public enum InterfaceLanguage
        {
            /// <summary>The game's language (English when the game's language is not one of these).</summary>
            Auto = 0,
            Arabic = 1,
            English = 2,
            /// <summary>Windows' display language (English when it is not one of these).</summary>
            Windows = 3,
            German = 4,
            Spanish = 5,
            French = 6,
            Italian = 7,
            Japanese = 8,
            Korean = 9,
            Polish = 10,
            PortugueseBrazil = 11,
            Russian = 12,
            ChineseSimplified = 13,
            ChineseTraditional = 14,
        }

        /// <summary>The language code of a fixed choice; null for Auto and Windows.</summary>
        public static string CodeOf(InterfaceLanguage choice)
        {
            switch (choice)
            {
                case InterfaceLanguage.Arabic: return "ar";
                case InterfaceLanguage.English: return "en";
                case InterfaceLanguage.German: return "de";
                case InterfaceLanguage.Spanish: return "es";
                case InterfaceLanguage.French: return "fr";
                case InterfaceLanguage.Italian: return "it";
                case InterfaceLanguage.Japanese: return "ja";
                case InterfaceLanguage.Korean: return "ko";
                case InterfaceLanguage.Polish: return "pl";
                case InterfaceLanguage.PortugueseBrazil: return "pt-BR";
                case InterfaceLanguage.Russian: return "ru";
                case InterfaceLanguage.ChineseSimplified: return "zh-HANS";
                case InterfaceLanguage.ChineseTraditional: return "zh-HANT";
                default: return null;
            }
        }

        /// <summary>The page language now: the player's choice, Windows', or the game's.</summary>
        public static string ResolveLanguage()
        {
            var choice = Mod.Settings != null ? Mod.Settings.Language : InterfaceLanguage.Auto;
            if (choice == InterfaceLanguage.Windows) return UiLanguages.Windows();
            return CodeOf(choice) ?? UiLanguages.Game();
        }

        /// <summary><paramref name="en"/> in the page language (<paramref name="ar"/> for Arabic), {name} placeholders filled from <paramref name="pairs"/>.</summary>
        internal static string Text(string en, string ar, params object[] pairs)
        {
            var language = ResolveLanguage();
            var template = language == UiLanguages.Arabic ? ar : UiLanguages.Translate(language, en);
            return pairs.Length == 0 ? template : UiLanguages.Format(template, pairs);
        }

        /// <summary>Adds the language list's entries: Auto and Windows in the page language, each language in its own.</summary>
        internal void AddLanguageNames(Dictionary<string, string> d, string auto, string windows)
        {
            foreach (InterfaceLanguage choice in System.Enum.GetValues(typeof(InterfaceLanguage)))
            {
                string name;
                if (choice == InterfaceLanguage.Auto) name = auto;
                else if (choice == InterfaceLanguage.Windows) name = windows;
                else name = UiLanguages.NativeNames[System.Array.IndexOf(UiLanguages.Codes, CodeOf(choice))];
                d[GetEnumValueLocaleID(choice)] = name;
            }
        }

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

        /// <summary>The language of this page; applied at once.</summary>
        [SettingsUISection(kSection, kLanguageGroup)]
        public InterfaceLanguage Language { get; set; } = InterfaceLanguage.Auto;

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
            Language = InterfaceLanguage.Auto;
        }

        public override void Apply()
        {
            base.Apply();
            // Names drawn with the old options are redrawn.
            Patches.LabelNamePatch.RefreshLabels();
            OptionsLocalization.ReloadIfStale();
        }
    }

    /// <summary>The Options page in Arabic.</summary>
    public sealed class LocaleAR : IDictionarySource
    {
        private readonly Setting m_Setting;

        public LocaleAR(Setting setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            var d = new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), "أسماء الشوارع العربية" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kLanguageGroup), "لغة الصفحة" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Language)), "لغة الصفحة" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Language)),
                    "لغة صفحة الإعدادات هذه فقط (أسماء الخريطة تبقى عربية). «تلقائي» يتبع لغة اللعبة، و«لغة ويندوز» يتبع لغة جهازك، أو اختر لغة بعينها. إذا لم تكن اللغة من لغات المود تظهر الإنجليزية." },
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
            m_Setting.AddLanguageNames(d, "تلقائي (لغة اللعبة)", "لغة ويندوز");
            return d;
        }

        public void Unload()
        {
        }
    }
    /// <summary>The Options page in English (the source of the other languages, see <see cref="UiLanguages.Translate"/>).</summary>
    public sealed class LocaleEN : IDictionarySource
    {
        private readonly Setting m_Setting;

        public LocaleEN(Setting setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            var d = new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), "Arabic Street Names" },
                { m_Setting.GetOptionTabLocaleID(Setting.kSection), "Main" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kLabelsGroup), "Map names" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kFontGroup), "Font" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup), "About" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kLanguageGroup), "Page language" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Language)), "Page language" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Language)),
                    "The language of this options page only (the map names stay Arabic). Auto follows the game's language, Windows language follows your computer, or pick one. A language the mod does not have shows in English." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Enabled)), "Enable the mod (Arabic names on the map)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Enabled)),
                    "Draws Arabic road and district names on the map with joined letters, right to left. Turning it off shows the map at once as the game draws it without the mod (useful for before and after pictures); turning it on brings the fix back at once, without restarting the game. Names in lists are not affected." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.KeepDiacritics)), "Keep diacritics (harakat)" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.KeepDiacritics)),
                    "Keeps fatha, damma, kasra and the other marks. The map label renderer cannot place them over the letters, so they are removed by default." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.WordSpacing)), "Space between words" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.WordSpacing)),
                    "Extra space between the words of a name only (it does not change the space between letters: Arabic letters are joined). A share of the letter size. Default 10%." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ArabicSize)), "Arabic letter size" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ArabicSize)),
                    "If letters run into each other inside a word, make this smaller: the game draws Arabic letters larger than their space. The default 60% matches the size of the other map names." },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Font)), "Fallback font" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Font)),
                    "Used for the joined letter forms when the game's fonts do not have them. Auto tries Arial, then Tahoma, then Segoe UI. Applied after restarting the game if a font was already added." },
                { m_Setting.GetEnumValueLocaleID(Setting.FallbackFont.Auto), "Auto" },
                { m_Setting.GetEnumValueLocaleID(Setting.FallbackFont.Arial), "Arial" },
                { m_Setting.GetEnumValueLocaleID(Setting.FallbackFont.Tahoma), "Tahoma" },
                { m_Setting.GetEnumValueLocaleID(Setting.FallbackFont.SegoeUI), "Segoe UI" },
                { m_Setting.GetEnumValueLocaleID(Setting.FallbackFont.None), "None (the game's fonts only)" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.FontStatus)), "Font status" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.FontStatus)), "What the font check found when map names were first drawn." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Version)), "Version" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.Version)), "The installed version of the mod." },
            };
            m_Setting.AddLanguageNames(d, "Auto (game language)", "Windows language");
            return d;
        }

        /// <summary>The page in <paramref name="code"/>: the English texts through <see cref="UiLanguages.Translate"/>.</summary>
        public Dictionary<string, string> ReadEntries(string code)
        {
            var english = (Dictionary<string, string>)ReadEntries(null, null);
            var d = new Dictionary<string, string>(english.Count);
            foreach (var pair in english) d[pair.Key] = UiLanguages.Translate(code, pair.Value);
            m_Setting.AddLanguageNames(d, UiLanguages.Translate(code, "Auto (game language)"), UiLanguages.Translate(code, "Windows language"));
            return d;
        }

        public void Unload()
        {
        }
    }

    /// <summary>One source per game locale that serves the page in its language (<see cref="Setting.ResolveLanguage"/>), whatever the game's language.</summary>
    public sealed class OptionsLocaleSource : IDictionarySource
    {
        private readonly LocaleAR m_Arabic;
        private readonly LocaleEN m_English;

        public OptionsLocaleSource(Setting setting)
        {
            m_Arabic = new LocaleAR(setting);
            m_English = new LocaleEN(setting);
        }

        /// <summary>The language last read into the game's dictionary; null before the first read.</summary>
        public string Served { get; private set; }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            var language = Setting.ResolveLanguage();
            Served = language;
            if (language == UiLanguages.Arabic) return m_Arabic.ReadEntries(errors, indexCounts);
            if (language == UiLanguages.English) return m_English.ReadEntries(errors, indexCounts);
            return m_English.ReadEntries(language);
        }

        public void Unload()
        {
        }
    }

    /// <summary>Registers the page text with every game locale and reloads it when its language changes.</summary>
    public static class OptionsLocalization
    {
        private static readonly Dictionary<string, OptionsLocaleSource> s_Sources = new Dictionary<string, OptionsLocaleSource>();
        private static Setting s_Setting;
        private static string s_LastReloadAttempt;
        private static bool s_Reloading;

        public static void Register(Setting setting)
        {
            s_Setting = setting;
            var locales = GameManager.instance?.localizationManager;
            if (locales == null) return;
            EnsureAllLocales();
            locales.onActiveDictionaryChanged += OnLocaleChanged;
            locales.onSupportedLocalesChanged += EnsureAllLocales;
        }

        private static void OnLocaleChanged() => ReloadIfStale();

        /// <summary>Adds a source to locales that have none yet (e.g. locales added by other mods after startup).</summary>
        public static void EnsureAllLocales()
        {
            if (s_Setting == null) return;
            var locales = GameManager.instance?.localizationManager;
            if (locales == null) return;
            foreach (var locale in locales.GetSupportedLocales())
            {
                if (s_Sources.ContainsKey(locale)) continue;
                var source = new OptionsLocaleSource(s_Setting);
                s_Sources[locale] = source;
                locales.AddSource(locale, source);
            }
        }

        /// <summary>Reloads the active game locale when its page text is in another language than the one chosen now.</summary>
        public static void ReloadIfStale()
        {
            if (s_Setting == null || s_Reloading) return;
            var locales = GameManager.instance?.localizationManager;
            if (locales == null) return;
            string active;
            try { active = locales.activeLocaleId; }
            catch { return; }
            if (string.IsNullOrEmpty(active)) return;
            if (!s_Sources.TryGetValue(active, out var source))
            {
                EnsureAllLocales();
                return;
            }
            var language = Setting.ResolveLanguage();
            if (source.Served == language) { s_LastReloadAttempt = null; return; }
            // One attempt per locale and language: a locale the game cannot reload is not retried.
            var attempt = active + "|" + language;
            if (attempt == s_LastReloadAttempt) return;
            s_LastReloadAttempt = attempt;
            s_Reloading = true;
            try { locales.ReloadActiveLocale(); }
            catch (System.Exception e) { Mod.Log.Warn(e, "Reloading the options text failed"); }
            finally { s_Reloading = false; }
        }

        /// <summary>Removes the page text from every game locale (mod unloaded).</summary>
        public static void Unregister()
        {
            var locales = GameManager.instance?.localizationManager;
            if (locales != null)
            {
                locales.onActiveDictionaryChanged -= OnLocaleChanged;
                locales.onSupportedLocalesChanged -= EnsureAllLocales;
                foreach (var pair in s_Sources) locales.RemoveSource(pair.Key, pair.Value);
            }
            s_Sources.Clear();
            s_Setting = null;
            s_LastReloadAttempt = null;
        }
    }
}
