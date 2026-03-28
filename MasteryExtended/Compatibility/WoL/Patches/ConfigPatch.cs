namespace MasteryExtended.Compatibility.WoL.Patches
{
    internal static class ConfigPatch
    {
        /***********
         * PATCHES *
         ***********/
        internal static bool alwaysFalsePrefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }
}