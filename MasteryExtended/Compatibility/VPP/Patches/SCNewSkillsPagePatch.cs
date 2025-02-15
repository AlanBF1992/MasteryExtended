using HarmonyLib;
using MasteryExtended.Patches;
using StardewModdingAPI;
using System.Reflection;
using System.Reflection.Emit;

namespace MasteryExtended.Compatibility.VPP.Patches
{
    internal static class SCNewSkillsPagePatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        internal static IEnumerable<CodeInstruction> OnRenderedActiveMenuTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                MethodInfo stringCompareInfo = AccessTools.Method(typeof(SkillsPagePatch), nameof(SkillsPagePatch.stringCompare));

                CodeMatcher matcher = new(instructions);

                // from: if (hoverText.Length > 0)
                // to:   if (hoverText.Length > 0 && !hoverText.Equals("MasteryExtended"))
                matcher
                    .MatchEndForward(
                        new CodeMatch(OpCodes.Nop),
                        new CodeMatch(OpCodes.Ldloc_S), //27
                        new CodeMatch(OpCodes.Callvirt),
                        new CodeMatch(OpCodes.Ldc_I4_0),
                        new CodeMatch(OpCodes.Cgt),
                        new CodeMatch(OpCodes.Stloc_S),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Brfalse_S)
                    )
                    .ThrowIfNotMatch("SCNewSkillsPagePatch.OnRenderedActiveMenuTranspiler: IL code 1 not found")
                ;

                Label lbl1 = (Label)matcher.Operand;

                matcher
                    .Advance(1)
                    .Insert(
                        new CodeInstruction(OpCodes.Ldloc_S, 27),
                        new CodeInstruction(OpCodes.Ldstr, "MasteryExtended"),
                        new CodeInstruction(OpCodes.Call, stringCompareInfo),
                        new CodeInstruction(OpCodes.Brtrue_S, lbl1)
                    )
                ;

                // from: if (gameMenu.hoverText.Length > 0)
                // to:   if (gameMenu.hoverText.Length > 0 && !gameMenu.hoverText.Equals("MasteryExtended"))
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Br_S),
                        new CodeMatch(OpCodes.Ldloc_0),
                        new CodeMatch(OpCodes.Ldfld),
                        new CodeMatch(OpCodes.Callvirt),
                        new CodeMatch(OpCodes.Ldc_I4_0),
                        new CodeMatch(OpCodes.Cgt),
                        new CodeMatch(OpCodes.Stloc_S),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Brfalse_S)
                    )
                    .ThrowIfNotMatch("SCNewSkillsPagePatch.OnRenderedActiveMenuTranspiler: IL code 2 not found")
                    .Advance(2)
                ;

                CodeInstruction gameHoverText = matcher.Instruction;

                matcher.Advance(6);

                Label lbl2 = (Label)matcher.Operand;

                matcher
                    .Advance(1)
                    .Insert(
                        new CodeInstruction(OpCodes.Ldloc_0),
                        gameHoverText,
                        new CodeInstruction(OpCodes.Ldstr, "MasteryExtended"),
                        new CodeInstruction(OpCodes.Call, stringCompareInfo),
                        new CodeInstruction(OpCodes.Brtrue_S, lbl2)
                    )
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(OnRenderedActiveMenuTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }
    }
}
