using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using System.Reflection;
using System.Reflection.Emit;

namespace MasteryExtended.Patches
{
    internal static class MineShaftPatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        public static IEnumerable<CodeInstruction> getFishTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);
                MethodInfo extraSpawnPercentageInfo = AccessTools.Method(typeof(MineShaftPatch), nameof(extraSpawnPercentage));

                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldstr, "Stonefish")
                    )
                ;

                for (int i = 0; i < 3; i++)
                {
                    matcher
                        .MatchStartForward(
                            new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)10)
                        )
                    ;
                    matcher.Opcode = OpCodes.Ldc_I4_S;
                    matcher.Operand = (sbyte)i;
                    matcher
                        .Advance(1)
                        .Insert(
                            new CodeInstruction(OpCodes.Ldarg_S, 4),
                            new CodeInstruction(OpCodes.Call, extraSpawnPercentageInfo)
                        )
                    ;
                }

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(getFishTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        private static double extraSpawnPercentage(int i, Farmer who)
        {
            if (!who.modData.TryGetValue($"{ModEntry.ModManifest.UniqueID}/ExtraMastery/BaitSpecialist", out string value)) return 10;

            double[] percentage = [32, 27.5, 23];
            return bool.Parse(value)? percentage[i]: 10;
        }
    }
}
