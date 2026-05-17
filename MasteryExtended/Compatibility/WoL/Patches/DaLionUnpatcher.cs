namespace MasteryExtended.Compatibility.WoL.Patches
{
    internal static class DaLionUnpatcher
    {
        /***********
         * PATCHES *
         ***********/
        internal static bool UnpatcherBoolPrefix(ref bool __result)
        {
            __result = true;
            return false;
        }

        internal static bool UnpatcherVoidPrefix => false;
    }
}
