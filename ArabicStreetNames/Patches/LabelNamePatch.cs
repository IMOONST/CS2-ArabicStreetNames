using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using ArabicStreetNames.Rendering;
using ArabicStreetNames.Text;
using Game.Rendering;
using Game.UI;
using HarmonyLib;
using Unity.Entities;

namespace ArabicStreetNames.Patches
{
    /// <summary>
    /// The map labels get their text from <see cref="NameSystem.GetRenderedLabelName"/> in exactly two places:
    /// AggregateMeshSystem.UpdateLabelVertices (road names) and AreaBufferSystem.UpdateLabelVertices (district
    /// names). A transpiler replaces those calls with <see cref="LabelText"/>. NameSystem itself is not patched, so
    /// every other caller (the game's UI, other mods) still gets the name as written.
    /// </summary>
    public static class LabelNamePatch
    {
        private static readonly MethodInfo s_Original = AccessTools.Method(typeof(NameSystem), nameof(NameSystem.GetRenderedLabelName), new[] { typeof(Entity) });
        private static readonly MethodInfo s_Replacement = AccessTools.Method(typeof(LabelNamePatch), nameof(LabelText));

        private static readonly (Type type, string method)[] s_Targets =
        {
            (typeof(AggregateMeshSystem), "UpdateLabelVertices"),
            (typeof(AreaBufferSystem), "UpdateLabelVertices"),
        };

        private static int s_Replaced;

        public static void Apply(Harmony harmony)
        {
            if (s_Original == null) throw new MissingMethodException(nameof(NameSystem), nameof(NameSystem.GetRenderedLabelName));
            var transpiler = new HarmonyMethod(AccessTools.Method(typeof(LabelNamePatch), nameof(Transpiler)));
            foreach (var (type, name) in s_Targets)
            {
                var target = AccessTools.Method(type, name);
                if (target == null)
                {
                    Mod.Log.Warn($"{type.Name}.{name} not found: its labels are not changed (game update?)");
                    continue;
                }
                var before = s_Replaced;
                harmony.Patch(target, transpiler: transpiler);
                Mod.Log.Info($"Patched {type.Name}.{name}: {s_Replaced - before} name call(s)");
            }
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (var instruction in instructions)
            {
                // instance + entity are on the stack either way; the static replacement takes them as arguments.
                if ((instruction.opcode == OpCodes.Callvirt || instruction.opcode == OpCodes.Call) && instruction.operand is MethodInfo m && m == s_Original)
                {
                    s_Replaced++;
                    yield return new CodeInstruction(OpCodes.Call, s_Replacement).MoveLabelsFrom(instruction).MoveBlocksFrom(instruction);
                    continue;
                }
                yield return instruction;
            }
        }

        /// <summary>The name for the map label: shaped and in visual order when it contains Arabic.</summary>
        public static string LabelText(NameSystem names, Entity entity)
        {
            var name = names.GetRenderedLabelName(entity);
            var settings = Mod.Settings;
            if (settings == null || !settings.Enabled || !ArabicBidi.ContainsRtl(name)) return name;
            try
            {
                var visual = ArabicText.ForLeftToRightRenderer(name, LabelFont.FormsAvailable, settings.KeepDiacritics);
                return LabelLayout.Apply(visual, settings.WordSpacing, settings.ArabicSize);
            }
            catch (Exception e)
            {
                Mod.Log.Error(e, $"Could not shape the label \"{name}\"");
                return name;
            }
        }

        /// <summary>Redraws every road and district label (after the options change).</summary>
        public static void RefreshLabels()
        {
            ArabicText.ClearCache();
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null) return;
            foreach (var (type, _) in s_Targets)
            {
                try
                {
                    var system = world.GetExistingSystemManaged(type);
                    var refresh = AccessTools.Method(type, "OnDictionaryChanged");
                    if (system != null && refresh != null) refresh.Invoke(system, null);
                }
                catch (Exception e)
                {
                    Mod.Log.Warn($"Could not refresh {type.Name} labels: {e.Message}");
                }
            }
        }
    }
}
