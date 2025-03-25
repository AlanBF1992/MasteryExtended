using HarmonyLib;
using MasteryExtended.Patches;
using StardewModdingAPI;
using System.Reflection;
using System.Reflection.Emit;

namespace MasteryExtended.Compatibility.VPP.Patches
{
    internal static class DisplayHandlerPatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;
        internal readonly static Type newSkillsPageType = AccessTools.TypeByName("SpaceCore.Interface.NewSkillsPage");

        internal static IEnumerable<CodeInstruction> OnRenderedActiveMenuTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            try
            {
                MethodInfo stringCompareInfo = AccessTools.Method(typeof(SkillsPagePatch), nameof(SkillsPagePatch.stringCompare));
                MethodInfo strLenInfo = AccessTools.PropertyGetter(typeof(string), nameof(string.Length));
                MethodInfo overlayValidInfo = AccessTools.Method("VanillaPlusProfessions.Utilities.CoreUtility:IsOverlayValid");

                CodeMatcher matcher = new(instructions, generator);

                matcher
                    .End()
                    .MatchStartBackwards(new CodeMatch(OpCodes.Call, overlayValidInfo))
                    .ThrowIfNotMatch("SCNewSkillsPagePatch.OnRenderedActiveMenuTranspiler: IL code 0 not found")
                    .CreateLabel(out Label skipAllDrawing)
                ;

                // from: if (c.containsPoint(Game1.getMouseX(true), Game1.getMouseY(true) + (skillScrollOffset * 56)) && c.hoverText.Length > 0 && !c.name.Equals("-1"))
                // to:   if (false)

                matcher
                    .Start()
                    .MatchStartForward(new CodeMatch(OpCodes.Ldstr, "-1"))
                    .ThrowIfNotMatch("SCNewSkillsPagePatch.OnRenderedActiveMenuTranspiler: IL code 1 not found")
                ;
                matcher
                    .Advance(2)
                    .Insert(
                        new CodeInstruction(OpCodes.Pop),
                        new CodeInstruction(OpCodes.Br_S, skipAllDrawing)
                    )
                ;

                // from: if (hoverText.Length > 0)
                // to:   if (hoverText.Length > 0 && !hoverText.Equals("MasteryExtended"))
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Callvirt, strLenInfo),
                        new CodeMatch(OpCodes.Ldc_I4_0)
                    )
                    .ThrowIfNotMatch("SCNewSkillsPagePatch.OnRenderedActiveMenuTranspiler: IL code 2 not found")
                ;

                var SCHoverTextInst = matcher.Instruction;

                matcher
                    .MatchStartForward(new CodeMatch(OpCodes.Newobj))
                    .ThrowIfNotMatch("SCNewSkillsPagePatch.OnRenderedActiveMenuTranspiler: IL code 2.1 not found")
                ;

                while (matcher.Advance(-1).Opcode == OpCodes.Nop) continue;

                Label lbl1 = (Label)matcher.Operand;

                matcher
                    .Advance(1)
                    .Insert(
                        SCHoverTextInst,
                        new CodeInstruction(OpCodes.Ldstr, "MasteryExtended"),
                        new CodeInstruction(OpCodes.Call, stringCompareInfo),
                        new CodeInstruction(OpCodes.Brtrue_S, lbl1)
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
