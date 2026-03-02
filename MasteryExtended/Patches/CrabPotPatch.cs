using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using System.Reflection;
using System.Reflection.Emit;
using Object = StardewValley.Object;

namespace MasteryExtended.Patches
{
    internal static class CrabPotPatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        // Better probability with Specific Bait
        internal static IEnumerable<CodeInstruction> DayUpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);

                MethodInfo extraProbabilityInfo = AccessTools.Method(typeof(CrabPotPatch), nameof(extraProbability));
                MethodInfo marinerProbabilityInfo = AccessTools.Method(typeof(CrabPotPatch), nameof(marinerProbability));

                // Without Mariner
                // from: chanceForCatch *= (double)((chanceForCatch < 0.1) ? 4 : ((chanceForCatch < 0.2) ? 3 : 2));
                // to:   chanceForCatch *= (double)((chanceForCatch < 0.1) ? 4 * extraProbability(3) : ((chanceForCatch < 0.2) ? 3 * extraProbability(2) : 2 * extraProbability(2)));
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldc_I4_2),
                        new CodeMatch(OpCodes.Br_S),
                        new CodeMatch(OpCodes.Ldc_I4_3)
                    )
                    .Advance(1)
                    .Insert(
                        new CodeInstruction(OpCodes.Ldloc_1),
                        new CodeInstruction(OpCodes.Ldc_I4_2),
                        new CodeInstruction(OpCodes.Call, extraProbabilityInfo),
                        new CodeInstruction(OpCodes.Mul)
                    )
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldc_I4_3)
                    )
                    .Advance(1)
                    .Insert(
                        new CodeInstruction(OpCodes.Ldloc_1),
                        new CodeInstruction(OpCodes.Ldc_I4_2),
                        new CodeInstruction(OpCodes.Call, extraProbabilityInfo),
                        new CodeInstruction(OpCodes.Mul)
                    )
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldc_I4_4)
                    )
                    .Advance(1)
                    .Insert(
                        new CodeInstruction(OpCodes.Ldloc_1),
                        new CodeInstruction(OpCodes.Ldc_I4_3),
                        new CodeInstruction(OpCodes.Call, extraProbabilityInfo),
                        new CodeInstruction(OpCodes.Mul)
                    )
                ;

                // With Mariner
                // add:

                matcher
                    .MatchEndForward(
                        new CodeMatch(OpCodes.Call),
                        new CodeMatch(OpCodes.Callvirt),
                        new CodeMatch(OpCodes.Ret)
                    )
                ;
                Console.WriteLine(matcher.Instruction);

                matcher
                    .Insert(
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldloc_1), // who
                        new CodeInstruction(OpCodes.Ldloc_S, 9), // baitTargetFish
                        new CodeInstruction(OpCodes.Ldloc_S, 4), // marinerList
                        new CodeInstruction(OpCodes.Ldloc_3),  // r
                        new CodeInstruction(OpCodes.Ldloc_S, 7), // quantity
                        new CodeInstruction(OpCodes.Ldloc_S, 8), // quality
                        new CodeInstruction(OpCodes.Call, marinerProbabilityInfo)
                    )
                ;


                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(DayUpdateTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        // Can put SpecificBait even with Mariner
        internal static bool performObjectDropInActionPrefix(CrabPot __instance, Item dropInItem, bool probe, Farmer who, ref bool __result)
        {
            if (!who.professions.Contains(11)) return true;
            if (!who.modData.TryGetValue($"{ModEntry.ModManifest.UniqueID}/ExtraMastery/Specialist", out string value) || !bool.Parse(value)) return true;

            GameLocation location = __instance.Location;
            if (location == null) return true;
            if (__instance.bait.Value != null) return true;

            if (dropInItem is not Object { QualifiedItemId: "(O)SpecificBait" } dropIn) return true;

            if (!probe)
            {
                if (who != null)
                {
                    __instance.owner.Value = who.UniqueMultiplayerID;
                }
                __instance.bait.Value = dropIn.getOne() as Object;
                location.playSound("Ship");
                __instance.lidFlapping = true;
                __instance.lidFlapTimer = 60f;
            }
            __result = true;
            return false;
        }

        private static void marinerProbability(CrabPot crabPot, Farmer who, string baitTargetFish, List<string> marinerList, Random r, int quantity, int quality)
        {
            if (!who.modData.TryGetValue($"{ModEntry.ModManifest.UniqueID}/ExtraMastery/Specialist", out string value) || !bool.Parse(value)) return;

            if (baitTargetFish != null && marinerList.Contains(baitTargetFish) && r.NextDouble() < 0.75)
            {
                crabPot.heldObject.Value = ItemRegistry.Create<Object>("(O)" + baitTargetFish, quantity, quality);
            }
        }

        private static int extraProbability(Farmer who, int extraMult)
        {
            if (!who.modData.TryGetValue($"{ModEntry.ModManifest.UniqueID}/ExtraMastery/Specialist", out string value)) return 1;
            return bool.Parse(value) ? extraMult : 1;
        }
    }
}
