
using HarmonyLib;
using System.Reflection.Emit;
using System.Reflection;

namespace MasteryExtended.VPP.Patches
{
    internal static class FarmerPatch
    {
        internal static IEnumerable<CodeInstruction> ShouldGainMasteryExpTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);
                MethodInfo farmerLevelInfo = AccessTools.Method(typeof(FarmerPatch), nameof(FarmerPatch.LevelToMastery));

                for (int i = 0; i < 5; i++)
                {
                    //from:  >= 10
                    //to:    >= MasteryCaveChanges()
                    matcher
                        .MatchStartForward(
                            new CodeMatch(OpCodes.Ldarg_0),
                            new CodeMatch(OpCodes.Ldfld),
                            new CodeMatch(OpCodes.Callvirt),
                            new CodeMatch(OpCodes.Ldc_I4_S),
                            new CodeMatch(OpCodes.Clt)

                        )
                        .ThrowIfNotMatch("Something Really Bad Happened: " + i )
                        .Advance(3)
                        .RemoveInstruction()
                        .Insert(
                            new CodeInstruction(OpCodes.Call, farmerLevelInfo)
                        )
                    ;
                }

                return matcher.InstructionEnumeration();
            }
            catch (Exception)
            {
                return instructions;
            }
        }

        private static int LevelToMastery()
        {
            return ModEntry.MasteryCaveChanges()? 20: 10;
        }
    }
}