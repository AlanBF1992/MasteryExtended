namespace MasteryExtended.Compatibility.WoL.Patches
{
    internal static class ConfigPatch
    {
        internal static bool alwaysFalsePrefix(ref bool __result)
        {
            __result = false;
            return false;
        }
    }
}