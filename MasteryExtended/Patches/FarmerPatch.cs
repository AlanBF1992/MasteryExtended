using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace MasteryExtended.Patches
{
    internal static class FarmerPatch
    {
        internal static IMonitor LogMonitor = ModEntry.LogMonitor;

        /// <summary>Permite ganar maestría en la habilidad que tenga nivel mayor a 10.</summary>
        internal static void gainExperiencePrefix(Farmer __instance, int which, int howMuch)
        {
            try
            {
                bool addMasteryPoints = false;

                if (__instance.Level < 25)
                {
                    switch (which)
                    {
                        case 0:
                            addMasteryPoints = __instance.farmingLevel.Value >= 10;
                            break;
                        case 3:
                            addMasteryPoints = __instance.miningLevel.Value >= 10;
                            break;
                        case 1:
                            addMasteryPoints = __instance.fishingLevel.Value >= 10;
                            break;
                        case 2:
                            addMasteryPoints = __instance.foragingLevel.Value >= 10;
                            break;
                        case 4:
                            addMasteryPoints = __instance.combatLevel.Value >= 10;
                            break;
                    }
                }

                if (addMasteryPoints)
                {
                    int old = MasteryTrackerMenu.getCurrentMasteryLevel();
                    Game1.stats.Increment("MasteryExp", which == 0? howMuch/2 : howMuch);
                    if (MasteryTrackerMenu.getCurrentMasteryLevel() > old)
                    {
                        Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:Mastery_newlevel"));
                        Game1.playSound("newArtifact");
                    }
                }
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(gainExperiencePrefix)}:\n{ex}", LogLevel.Error);
            }
        }
    }
}
