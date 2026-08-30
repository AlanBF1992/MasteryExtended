using HarmonyLib;
using StardewModdingAPI;
using System.Reflection;
using System.Reflection.Emit;

namespace MasteryExtended.Compatibility.WoL.Patches
{
    internal static class GameLocationPerformActionPatcherPatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        internal static IEnumerable<CodeInstruction> GameLocationPerformActionPrefixTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                MethodInfo getLimitBreaksInfo = AccessTools.PropertyGetter("DaLion.Professions.ProfessionsMod:ShouldEnableLimitBreaks");

                CodeMatcher matcher = new(instructions);

                // From: Do things with MasteryRoom and DogStatue
                // To:   Don't
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Call, getLimitBreaksInfo)
                    )
                    .MatchStartBackwards(
                        new CodeMatch(OpCodes.Ret)
                    )
                    .ThrowIfNotMatch("GameLocationPerformActionPatcherPatch.GameLocationPerformActionPrefixTranspiler: IL code not found")
                    .Advance(1)
                ;

                matcher.Opcode = OpCodes.Ldc_I4_1;
                matcher.Operand = null;
                matcher.Advance(1);

                matcher.Opcode = OpCodes.Ret;
                matcher.Operand = null;
                matcher.Advance(1);

                matcher.RemoveInstructions(matcher.Remaining);

                return matcher.InstructionEnumeration();

            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(GameLocationPerformActionPrefixTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }
    }
}
