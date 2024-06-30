using HarmonyLib;
using MasteryExtended.Skills.Professions;
using MasteryExtended.WoL.Patches;
using StardewModdingAPI;

namespace MasteryExtended.WoL
{
    public class ModEntry : Mod
    {
        /// <summary>Log info.</summary>
        public static IMonitor LogMonitor { get; private set; } = null!;
        // Helper
        public static IModHelper ModHelper { get; private set; } = null!;

        public static List<int> ProfessionsToSave { get; } = new();

        public override void Entry(IModHelper helper)
        {
            ModHelper = helper;
            LogMonitor = Monitor; // Solo aca empieza a existir

            // Insertar Parches necesarios
            var harmony = new Harmony(this.ModManifest.UniqueID);
            applyPatches(harmony);
        }

        private void applyPatches(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Profession), nameof(Profession.AddProfessionToPlayer)),
                postfix: new HarmonyMethod(typeof(ProfessionPatch), nameof(ProfessionPatch.AddProfessionToPlayerPostfix))
            );
            harmony.Patch(
                original: AccessTools.Method("DaLion.Professions.Framework.Events.GameLoop.ProfessionSavingEvent:OnSavingImpl"),
                postfix: new HarmonyMethod(typeof(ProfessionSavingEventPatch), nameof(ProfessionSavingEventPatch.OnSavingImplPostfix))
            );
        }
    }
}
