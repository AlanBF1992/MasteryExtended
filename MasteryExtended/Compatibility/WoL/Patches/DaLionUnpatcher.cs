namespace MasteryExtended.Compatibility.WoL.Patches
{
    internal static class DaLionUnpatcher
    {
        internal static bool UnpatcherBoolPrefix(ref bool __result)
        {
            //Dalion Patch is asigned a return of true and skipped;
            __result = true;
            return false;
        }

        internal static bool UnpatcherVoidPrefix()
        {
            //Dalion Patch is skipped;
            return false;
        }
    }
}
