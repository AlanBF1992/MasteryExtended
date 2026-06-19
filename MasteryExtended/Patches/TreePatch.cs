using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Reflection;
using System.Reflection.Emit;

namespace MasteryExtended.Patches
{
    internal static class TreePatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        /***********
         * PATCHES *
         ***********/
        internal static IEnumerable<CodeInstruction> dayUpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);
                MethodInfo checkGrowthStageInfo = AccessTools.Method(typeof(TreePatch), nameof(checkGrowthStage));

                // From: growthStage.Value++
                // To:   checkGrowthStage(this)
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldfld),
                        new CodeMatch(OpCodes.Dup)
                    )
                    .ThrowIfNotMatch("TreePatch.dayUpdateTranspiler: IL code not found")
                    .Advance(1)
                    .RemoveInstructions(8)
                    .Insert(
                        new CodeInstruction(OpCodes.Call, checkGrowthStageInfo)
                    )
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(dayUpdateTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        internal static IEnumerable<CodeInstruction> tickUpdateTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);
                MethodInfo getItemIdInfo = AccessTools.PropertyGetter(typeof(Item), nameof(Item.ItemId));
                MethodInfo extraHardwoodInfo = AccessTools.Method(typeof(TreePatch), nameof(extraHardwood));

                // From: if (drop.ItemId == "709")
                // To:   if (item.ItemId == "709")
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldstr, "709")
                    )
                    .ThrowIfNotMatch("TreePatch.tickUpdateTranspiler: IL code 1 not found")
                    .Advance(-2)
                    .SetOperandAndAdvance(19)
                    .SetOperandAndAdvance(getItemIdInfo)
                ;

                // From: numHardwood += item.Stack
                // To:   numHardwood += item.Stack + extraHardwood(this)
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Stloc_S)
                    )
                    .ThrowIfNotMatch("TreePatch.tickUpdateTranspiler: IL code 2 not found")
                    .Insert(
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Call, extraHardwoodInfo),
                        new CodeInstruction(OpCodes.Add)
                    )
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(tickUpdateTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        internal static void extraWoodCalculatorPostfix(Tree __instance, ref int __result)
        {
            if (__instance.growthStage.Value >= 5 && IsFertilizedByWoodlander(__instance))
            {
                __result++;
            }
        }

        internal static bool IsGrowthBlockedByNearbyTreePrefix(Tree __instance, ref bool __result)
        {
            if (IsFertilizedByWoodlander(__instance))
            {
                __result = false;
                return false;
            }
            return true;
        }

        /***********
         * METHODS *
         ***********/
        internal static bool isFarmerWoodlander(Farmer who)
        {
            return ModEntry.Config.EnableDogPowers
                && who.modData.TryGetValue($"{ModEntry.ModManifest.UniqueID}/ExtraMastery/Woodlander", out string value)
                && bool.Parse(value);
        }

        internal static bool IsFertilizedByWoodlander(Tree tree)
        {
            return tree.fertilized.Value
                && tree.modData.TryGetValue($"{ModEntry.ModManifest.UniqueID}/TreeData/FertilizedBy", out string stringId)
                && long.TryParse(stringId, out long id)
                && Game1.GetPlayer(id) is Farmer who
                && isFarmerWoodlander(who);
        }

        internal static void checkGrowthStage(Tree tree)
        {
            int baseGrowth = IsFertilizedByWoodlander(tree)? 3: 1;
            tree.growthStage.Value = Math.Min(tree.GetMaxSizeHere(), tree.growthStage.Value + baseGrowth);
        }
        internal static int extraHardwood(Tree tree)
        {
            if (tree.growthStage.Value >= 5 && IsFertilizedByWoodlander(tree))
            {
                return 1;
            }
            return 0;
        }
    }
}
