using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace MasteryExtended.SC.Patches
{
    internal static class SkillsPatch
    {
        internal static IMonitor LogMonitor = ModEntry.LogMonitor;

        internal static void AddExperiencePrefix(Farmer farmer, string skillName, int amt)
        {
            int skillLevel = SpaceCore.Skills.GetSkillLevel(farmer, skillName);
            if (skillLevel >= 10 && farmer.Level < 25)
            {
                int old = MasteryTrackerMenu.getCurrentMasteryLevel();
                // No idea why, but SpaceCore adds only half
                Game1.stats.Increment("MasteryExp", Math.Max(1, amt / 2));
                if (MasteryTrackerMenu.getCurrentMasteryLevel() > old)
                {
                    Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:Mastery_newlevel"));
                    Game1.playSound("newArtifact");
                }
            }
        }
    }
}
