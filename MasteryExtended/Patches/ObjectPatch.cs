using HarmonyLib;
using StardewModdingAPI;
using StardewValley;
using StardewValley.TerrainFeatures;
using System.Reflection;
using System.Reflection.Emit;

namespace MasteryExtended.Patches
{
    internal static class ObjectPatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        /***********
         * PATCHES *
         ***********/
        public static IEnumerable<CodeInstruction> placementActionTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);
                MethodInfo fertilizeInfo = AccessTools.Method(typeof(Tree), nameof(Tree.fertilize));
                MethodInfo addModDataFertilizerInfo = AccessTools.Method(typeof(ObjectPatch), nameof(addModDataFertilizer));

                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Callvirt, fertilizeInfo)
                    )
                    .Advance(1)
                    .Insert(
                        new CodeInstruction(OpCodes.Ldarg_S, 4),
                        new CodeInstruction(OpCodes.Ldloc_S, 23),
                        new CodeInstruction(OpCodes.Call, addModDataFertilizerInfo)
                    )
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(placementActionTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        /***********
         * METHODS *
         ***********/
        private static bool addModDataFertilizer(bool result, Farmer? who, Tree tree)
        {
            if(result && who is not null)
            {
                tree.modData.TryAdd($"{ModEntry.ModManifest.UniqueID}/TreeData/FertilizedBy", who.UniqueMultiplayerID.ToString());
            }

            return result;
        }
    }
}
