
using StardewValley;

namespace MasteryExtended.WoL.Patches
{
    internal static class VanillaSkillPatch
    {
        internal static void OnSaveLoadedImplPostfix()
        {
            Game1.player.newLevels.Clear();
            //En teoría debería revisar las skills y ver cuál se está intentando agregar
            //En la realidad, ninguna debería intentar agregarse, así que fuck them
        }
    }
}