using StardewModdingAPI;
using MasteryExtended.Skills.Professions;

namespace MasteryExtended.WoL.Patches
{
    internal static class ProfessionPatch
    {
        internal static IMonitor LogMonitor = ModEntry.LogMonitor;

        internal static void AddProfessionToPlayerPostfix(Profession __instance)
        {
            ModEntry.ProfessionsToSave.Add(__instance.Id);
        }
    }
}
