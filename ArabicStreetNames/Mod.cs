using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using HarmonyLib;

namespace ArabicStreetNames
{
    /// <summary>
    /// Arabic Street Names: road and district names on the map are drawn by TextMeshPro, which has no Arabic shaping
    /// or right-to-left layout. The two calls that hand a name to that renderer are patched (Patches/LabelNamePatch.cs)
    /// to pass the name shaped and in visual order instead (Text/ArabicText.cs).
    /// </summary>
    public sealed class Mod : IMod
    {
        public const string HarmonyId = "ArabicStreetNames";
        public static readonly ILog Log = LogManager.GetLogger(nameof(ArabicStreetNames)).SetShowsErrorsInUI(false);

        public static Setting Settings { get; private set; }
        private Harmony m_Harmony;

        public void OnLoad(UpdateSystem updateSystem)
        {
            Log.Info(nameof(OnLoad));

            Settings = new Setting(this);
            Settings.RegisterInOptionsUI();
            AssetDatabase.global.LoadSettings(nameof(ArabicStreetNames), Settings, new Setting(this));
            OptionsLocalization.Register(Settings);

            m_Harmony = new Harmony(HarmonyId);
            Patches.LabelNamePatch.Apply(m_Harmony);
        }

        public void OnDispose()
        {
            Log.Info(nameof(OnDispose));
            m_Harmony?.UnpatchAll(HarmonyId);
            m_Harmony = null;
            OptionsLocalization.Unregister();
            if (Settings != null)
            {
                Settings.UnregisterInOptionsUI();
                Settings = null;
            }
        }
    }
}
