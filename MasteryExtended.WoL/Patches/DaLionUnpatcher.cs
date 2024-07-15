
namespace MasteryExtended.WoL.Patches
{
    internal static class DaLionUnpatcher
    {
        internal static bool LevelUpMenuUpdateUnpatcherPrefix(ref bool __result)
        {
            //Dalion Patch is asigned a return of true and skipped;
            __result = true;
            return false;
        }
    }
}
