using HarmonyLib;
using SpaceCore;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Reflection.Emit;

namespace MasteryExtended.SC.Patches
{
    internal static class SkillsPatch
    {
        internal static IMonitor LogMonitor = ModEntry.LogMonitor;

        internal static IEnumerable<CodeInstruction> AddExperienceTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);

                //from: if (prevLevel >= 10 && level >= 25)
                //to:   if (prevLevel >= 10)
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldloc_1),
                        new CodeMatch(OpCodes.Ldc_I4_S),
                        new CodeMatch(OpCodes.Blt_S)
                    )
                    .ThrowIfNotMatch("SpaceCore AddExperienceTranspiler: IL code not found")
                    .RemoveInstructions(3)
                ;

                return matcher.InstructionEnumeration();
            } catch (Exception)
            {
                return instructions;
            }
        }
    }
}
