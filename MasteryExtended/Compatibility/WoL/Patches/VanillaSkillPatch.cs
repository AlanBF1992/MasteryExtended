using StardewValley;

namespace MasteryExtended.Compatibility.WoL.Patches
{
    internal static class VanillaSkillPatch
    {
        internal static void OnSaveLoadedImplPostfix()
        {
            Game1.player.newLevels.Clear();
            // In theory I should check skills and see which one is being added
            // In reality no external skills should attempt self-registration
        }
    }
}