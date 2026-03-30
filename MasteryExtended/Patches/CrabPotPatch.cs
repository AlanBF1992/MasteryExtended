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

        /***********
         * PATCHES *
         ***********/
        internal static IEnumerable<CodeInstruction> DayUpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);

                MethodInfo extraProbabilityInfo = AccessTools.Method(typeof(CrabPotPatch), nameof(extraProbability));
                MethodInfo marinerProbabilityInfo = AccessTools.Method(typeof(CrabPotPatch), nameof(marinerProbability));

                // Without Mariner
                // From: chanceForCatch *= (double)((chanceForCatch < 0.1) ? 4 : ((chanceForCatch < 0.2) ? 3 : 2));
                // To:   chanceForCatch *= (double)((chanceForCatch < 0.1) ? 4 * extraProbability(3) : ((chanceForCatch < 0.2) ? 3 * extraProbability(2) : 2 * extraProbability(2)));
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldc_I4_2),
                        new CodeMatch(OpCodes.Br_S),
                        new CodeMatch(OpCodes.Ldc_I4_3)
                    )
                    .ThrowIfNotMatch("CrabPotPatch.DayUpdateTranspiler: IL code 1 not found")
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
                    .ThrowIfNotMatch("CrabPotPatch.DayUpdateTranspiler: IL code 2 not found")
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
                    .ThrowIfNotMatch("CrabPotPatch.DayUpdateTranspiler: IL code 3 not found")
                    .Advance(1)
                    .Insert(
                        new CodeInstruction(OpCodes.Ldloc_1),
                        new CodeInstruction(OpCodes.Ldc_I4_3),
                        new CodeInstruction(OpCodes.Call, extraProbabilityInfo),
                        new CodeInstruction(OpCodes.Mul)
                    )
                ;

                // With Mariner
                // Add: marinerProbability(this, who, baitTargetFish, marinerList, r, quantity, quality)
                matcher
                    .MatchEndForward(
                        new CodeMatch(OpCodes.Call),
                        new CodeMatch(OpCodes.Callvirt),
                        new CodeMatch(OpCodes.Ret)
                    )
                    .ThrowIfNotMatch("CrabPotPatch.DayUpdateTranspiler: IL code 4 not found")
                ;

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

        internal static bool performObjectDropInActionPrefix(CrabPot __instance, Item dropInItem, bool probe, Farmer who, ref bool __result)
        {
            if (!isFarmerBaitbinder(who)) return true;

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

        /***********
         * METHODS *
         ***********/
        internal static void marinerProbability(CrabPot crabPot, Farmer who, string baitTargetFish, List<string> marinerList, Random r, int quantity, int quality)
        {
            if (!isFarmerBaitbinder(who)) return;

            if (baitTargetFish != null && marinerList.Contains(baitTargetFish) && r.NextDouble() < 0.75)
            {
                crabPot.heldObject.Value = ItemRegistry.Create<Object>("(O)" + baitTargetFish, quantity, quality);
            }
        }

        internal static int extraProbability(Farmer who, int extraMult)
        {
            if (!isFarmerBaitbinder(who)) return 1;
            return extraMult;
        }

        internal static bool isFarmerBaitbinder(Farmer who)
        {
            //who ??= Game1.player;
            return who is not null && who.modData.TryGetValue($"{ModEntry.ModManifest.UniqueID}/ExtraMastery/Baitbinder", out string value)
                && bool.Parse(value);
        }
    }
}
