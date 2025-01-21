using DaLion.Professions.Framework;
using HarmonyLib;
using StardewValley;
using System.Reflection;
using System.Reflection.Emit;

namespace MasteryExtended.WoL.Patches
{
    internal static class GainExperiencePatch
    {
        internal static IEnumerable<CodeInstruction> FarmerGainExperiencePrefixTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            try
            {
                CodeMatcher matcher = new(instructions, generator);

                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Call),
                        new CodeMatch(OpCodes.Brfalse_S),
                        new CodeMatch(OpCodes.Call),
                        new CodeMatch(OpCodes.Stloc_2)
                    )
                    .ThrowIfNotMatch("WoL FarmerGainExperiencePrefixTranspiler: IL code not found")
                    .Advance(2)
                    .CreateLabel(out Label alwaysTrue)
                    .Advance(-1)
                    .Set(OpCodes.Brfalse_S, alwaysTrue)
                ;
                return matcher.InstructionEnumeration();
            } catch (Exception)
            {
                return instructions;
            }
        }
    }
}
