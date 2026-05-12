using HarmonyLib;
using StardewModdingAPI;
using System.Reflection;
using System.Reflection.Emit;

namespace MasteryExtended.Compatibility.WoL.Patches
{
    internal static class RevalidateBuildingsDayStartedEventPatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        /***********
         * PATCHES *
         ***********/
        internal static IEnumerable<CodeInstruction> OnDayStartedImplTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            try
            {
                CodeMatcher matcher = new(instructions, generator);


                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Stfld),
                        new CodeMatch(OpCodes.Br_S)
                    )
                    .ThrowIfNotMatch("RevalidateBuildingsDayStartedEventPatch.OnDayStartedImplTranspiler: IL code not found")
                    .Advance(1)
                    .RemoveInstruction()
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(OnDayStartedImplTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

    }
}
