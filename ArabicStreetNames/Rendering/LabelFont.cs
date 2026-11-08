using System;
using System.Collections.Generic;
using Game.Rendering;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace ArabicStreetNames.Rendering
{
    /// <summary>
    /// Makes sure the font of the map labels can draw Arabic presentation forms (the connected letter shapes the
    /// shaper produces). The labels use the TextMeshPro object of <see cref="OverlayRenderSystem"/>, whose fonts are
    /// dynamic font assets built from the game's fonts. Checked once, on the first label drawn:
    /// 1. the game's fonts (and their fallbacks) already have the forms → nothing to add;
    /// 2. otherwise a Windows font that has them (Arial, Tahoma, Segoe UI) becomes the first fallback, built exactly
    ///    like the game's own label fonts (SDF, dynamic atlas);
    /// 3. otherwise <see cref="FormsAvailable"/> is false and names are only put in reading order, unshaped
    ///    (separate letters, but in the right order) rather than drawn as missing-glyph boxes.
    /// </summary>
    public static class LabelFont
    {
        /// <summary>Letters checked: initial lam, lam-alef, initial beh, medial meem, final heh, final yeh, medial ain.</summary>
        private static readonly char[] s_Probe = { 'ﻟ', 'ﻻ', 'ﺑ', 'ﻤ', 'ﻪ', 'ﻲ', 'ﻌ' };

        private static bool s_Checked;
        private static bool s_FormsAvailable;
        private static Func<string> s_Status = () => "";

        public static bool FormsAvailable
        {
            get
            {
                if (!s_Checked) Check();
                return s_FormsAvailable;
            }
        }

        /// <summary>Shown on the Options page.</summary>
        public static string StatusText => s_Checked ? s_Status() : Setting.Text("No names drawn yet (open a city)", "لم تُرسم أسماء بعد (افتح مدينة)");

        private static void Check()
        {
            s_Checked = true;
            try
            {
                var overlay = World.DefaultGameObjectInjectionWorld?.GetExistingSystemManaged<OverlayRenderSystem>();
                var font = overlay?.GetTextMesh()?.font;
                if (font == null)
                {
                    // Not ready yet: check again on the next label.
                    s_Checked = false;
                    return;
                }

                if (HasForms(font))
                {
                    s_FormsAvailable = true;
                    s_Status = () => Setting.Text("The game's fonts have the joined letter forms", "خطوط اللعبة تحتوي الحروف المتصلة");
                    Mod.Log.Info("Label font: the game's fonts have the Arabic presentation forms");
                    return;
                }

                var choice = Mod.Settings?.Font ?? Setting.FallbackFont.Auto;
                if (choice != Setting.FallbackFont.None)
                {
                    foreach (var name in Candidates(choice))
                    {
                        var added = AddFallback(font, name);
                        if (added != null && HasForms(font))
                        {
                            s_FormsAvailable = true;
                            s_Status = () => Setting.Text("Fallback font added: {name}", "أُضيف الخط الاحتياطي: {name}", "name", name);
                            Mod.Log.Info($"Label font: added {name} as a fallback for the Arabic presentation forms");
                            return;
                        }
                        if (added != null) font.fallbackFontAssetTable.Remove(added);
                    }
                }

                s_FormsAvailable = false;
                s_Status = () => Setting.Text("No font with the joined letter forms: names show in the right order with separate letters", "لا يوجد خط بالحروف المتصلة: تُعرض الأسماء بالترتيب الصحيح وحروف منفصلة");
                Mod.Log.Warn("Label font: no font with Arabic presentation forms; names are reordered but not shaped");
            }
            catch (Exception e)
            {
                s_FormsAvailable = false;
                var message = e.Message;
                s_Status = () => Setting.Text("The font check failed: {error}", "تعذر فحص الخط: {error}", "error", message);
                Mod.Log.Error(e, "Label font check failed");
            }
        }

        private static IEnumerable<string> Candidates(Setting.FallbackFont choice)
        {
            switch (choice)
            {
                case Setting.FallbackFont.Arial: return new[] { "Arial" };
                case Setting.FallbackFont.Tahoma: return new[] { "Tahoma" };
                case Setting.FallbackFont.SegoeUI: return new[] { "Segoe UI" };
                default: return new[] { "Arial", "Tahoma", "Segoe UI" };
            }
        }

        /// <summary>Whether the font or its fallbacks can draw every probe letter (adding them to dynamic atlases).</summary>
        private static bool HasForms(TMP_FontAsset font)
        {
            foreach (var c in s_Probe)
            {
                if (!font.HasCharacter(c, searchFallbacks: true, tryAddCharacter: true)) return false;
            }
            return true;
        }

        /// <summary>A dynamic SDF font asset from an installed Windows font, inserted as the first fallback.</summary>
        private static TMP_FontAsset AddFallback(TMP_FontAsset font, string osFontName)
        {
            try
            {
                var source = Font.CreateDynamicFontFromOSFont(osFontName, 90);
                if (source == null) return null;
                // The same parameters the game uses for its label fonts (OverlayRenderSystem.CreateFont).
                var asset = TMP_FontAsset.CreateFontAsset(source, 90, 9, GlyphRenderMode.SDFAA_HINTED, 1024, 1024, AtlasPopulationMode.Dynamic, true);
                if (asset == null) return null;
                asset.name = "Arabic labels (" + osFontName + ")";
                asset.material.SetFloat(ShaderUtilities.ID_FaceDilate, 1f);
                font.fallbackFontAssetTable ??= new List<TMP_FontAsset>();
                font.fallbackFontAssetTable.Insert(0, asset);
                return asset;
            }
            catch (Exception e)
            {
                Mod.Log.Warn($"Label font: {osFontName} could not be used: {e.Message}");
                return null;
            }
        }
    }
}
