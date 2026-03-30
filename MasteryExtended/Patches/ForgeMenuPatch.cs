using StardewModdingAPI;
using StardewValley;
using StardewValley.Tools;

namespace MasteryExtended.Patches
{
    internal static class ForgeMenuPatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        /***********
         * PATCHES *
         ***********/
        internal static void GetForgeCostPostfix(Item left_item, ref int __result)
        {
            Farmer who = left_item is MeleeWeapon weapon ? weapon.lastUser ?? Game1.player : Game1.player;
            if (!MeleeWeaponPatch.isFarmerRunesmith(who)) return;

            __result = (__result + 1) / 2;
        }
    }
}
