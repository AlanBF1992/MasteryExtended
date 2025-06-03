using HarmonyLib;
using StardewModdingAPI;
using System.Reflection.Emit;

namespace MasteryExtended.Compatibility.SpaceCore.Patches
{
    internal static class SCSkillsPatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

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
                    .ThrowIfNotMatch("SCSkillsPatch.AddExperienceTranspiler: IL code not found")
                    .RemoveInstructions(3)
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(AddExperienceTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }
    }
}
