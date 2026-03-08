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

        /***********
         * PATCHES *
         ***********/
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

        public static IEnumerable<CodeInstruction> checkStoneForItemsTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);
                MethodInfo spawnStoneDebrisWithOreInfo = AccessTools.Method(typeof(MineShaftPatch), nameof(spawnStoneDebrisWithOre));
                MethodInfo shouldSpawnDebrisInfo = AccessTools.Method(typeof(MineShaftPatch), nameof(shouldSpawnDebris));
                MethodInfo spawnExtraDebrisInfo = AccessTools.Method(typeof(MineShaftPatch), nameof(spawnExtraDebris));


                matcher
                    .End()
                    .MatchEndBackwards(
                        new CodeMatch(OpCodes.Callvirt),
                        new CodeMatch(OpCodes.Ret)
                    )
                    .Insert(
                        new CodeInstruction(OpCodes.Ldarg_S, 4),
                        new CodeInstruction(OpCodes.Ldarg_2),
                        new CodeInstruction(OpCodes.Ldarg_3),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Call, spawnStoneDebrisWithOreInfo)
                    )
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Brfalse_S)
                    )
                    .Insert(
                        new CodeInstruction(OpCodes.Ldarg_S, 4),
                        new CodeInstruction(OpCodes.Call, shouldSpawnDebrisInfo)
                    )
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ret)
                    )
                    .Insert(
                        new CodeInstruction(OpCodes.Ldarg_S, 4),
                        new CodeInstruction(OpCodes.Ldloc_S, 4),
                        new CodeInstruction(OpCodes.Ldarg_2),
                        new CodeInstruction(OpCodes.Ldarg_3),
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Call, spawnExtraDebrisInfo)
                    )
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(checkStoneForItemsTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        /***********
         * METHODS *
         ***********/
        // Bait Specialist
        private static double extraSpawnPercentage(int i, Farmer? who)
        {
            if (who is null
                || !who.modData.TryGetValue($"{ModEntry.ModManifest.UniqueID}/ExtraMastery/BaitSpecialist", out string value)
                || !bool.Parse(value))
            {
                return 10;
            }

            double[] percentage = [32, 27.5, 23];
            return percentage[i];
        }

        // Mason
        private static void spawnStoneDebrisWithOre(Farmer? who, int x, int y, GameLocation location)
        {
            if (who is null
                || !who.modData.TryGetValue($"{ModEntry.ModManifest.UniqueID}/ExtraMastery/Mason", out string value)
                || !bool.Parse(value))
            {
                return;
            }

            Game1.createDebris(14, x, y, 1, location);
        }

        private static bool shouldSpawnDebris(bool defaultBool, Farmer? who)
        {
            if (who is null
                || !who.modData.TryGetValue($"{ModEntry.ModManifest.UniqueID}/ExtraMastery/Mason", out string value)
                || !bool.Parse(value))
            {
                return defaultBool;
            }

            return true;
        }

        private static void spawnExtraDebris(Farmer? who, Random r, int x, int y, GameLocation location)
        {
            if (who is null
                || !who.modData.TryGetValue($"{ModEntry.ModManifest.UniqueID}/ExtraMastery/Mason", out string value)
                || !bool.Parse(value))
            {
                return;
            }

            long farmerId = who?.UniqueMultiplayerID ?? 0;
            Game1.createObjectDebris("(O)390", x, y, farmerId, location); // Stone

            switch (r.NextDouble())
            {
                case < 0.5:
                    Game1.createObjectDebris("(O)330", x, y, farmerId, location); // Clay
                    break;
                case < 0.6:
                    Game1.createObjectDebris("(O)567", x, y, farmerId, location); // Marble

                    break;
                case < 0.65:
                    Game1.createObjectDebris("(O)568", x, y, farmerId, location); // Sandstone
                    break;
                case < 0.70:
                    Game1.createObjectDebris("(O)569", x, y, farmerId, location); // Granite
                    break;
                case < 0.75:
                    Game1.createObjectDebris("(O)571", x, y, farmerId, location); // Granite
                    break;
            }
        }
    }
}
