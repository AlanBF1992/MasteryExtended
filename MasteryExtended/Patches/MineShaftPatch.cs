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
        internal static IEnumerable<CodeInstruction> getFishTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);
                MethodInfo extraSpawnPercentageInfo = AccessTools.Method(typeof(MineShaftPatch), nameof(extraSpawnPercentage));

                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldstr, "Stonefish")
                    )
                    .ThrowIfNotMatch("MineShaftPatch.getFishTranspiler: IL code 1 not found")
                ;

                for (int i = 0; i < 3; i++)
                {
                    matcher
                        .MatchStartForward(
                            new CodeMatch(OpCodes.Ldc_I4_S, (sbyte)10)
                        )
                        .ThrowIfNotMatch($"MineShaftPatch.getFishTranspiler: IL code {i + 2} not found")
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

        internal static IEnumerable<CodeInstruction> checkStoneForItemsTranspiler(IEnumerable<CodeInstruction> instructions)
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
                    .ThrowIfNotMatch("MineShaftPatch.checkStoneForItemsTranspiler: IL code 1 not found")
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
                    .ThrowIfNotMatch("MineShaftPatch.checkStoneForItemsTranspiler: IL code 2 not found")
                    .Insert(
                        new CodeInstruction(OpCodes.Ldarg_S, 4),
                        new CodeInstruction(OpCodes.Call, shouldSpawnDebrisInfo)
                    )
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ret)
                    )
                    .ThrowIfNotMatch("MineShaftPatch.checkStoneForItemsTranspiler: IL code 3 not found")
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
        // Baitbinder
        internal static double extraSpawnPercentage(int i, Farmer? who)
        {
            if (who is null || !CrabPotPatch.isFarmerBaitbinder(who))
            {
                return 10;
            }

            double[] percentage = [32, 27.5, 23];
            return percentage[i];
        }

        // Mason
        internal static void spawnStoneDebrisWithOre(Farmer? who, int x, int y, GameLocation location)
        {
            if (who is null || !isFarmerMason(who))
            {
                return;
            }

            Game1.createDebris(14, x, y, 1, location);
        }

        internal static bool shouldSpawnDebris(bool defaultBool, Farmer? who)
        {
            if (who is null || !isFarmerMason(who))
            {
                return defaultBool;
            }

            return true;
        }

        internal static void spawnExtraDebris(Farmer? who, Random r, int x, int y, GameLocation location)
        {
            if (who is null || !isFarmerMason(who))
            {
                return;
            }

            long farmerId = who.UniqueMultiplayerID;

            Game1.createObjectDebris("(O)390", x, y, farmerId, location); // Stone

            switch (r.NextDouble())
            {
                case < 0.8:
                    Game1.createObjectDebris("(O)330", x, y, farmerId, location); // Clay
                    break;
                case double _ when ModEntry.Config.MasonDrops != MasonDropsOption.Everything:
                    break;
                case < 0.85:
                    Game1.createObjectDebris("(O)567", x, y, farmerId, location); // Marble
                    break;
                case < 0.9:
                    Game1.createObjectDebris("(O)568", x, y, farmerId, location); // Sandstone
                    break;
                case < 0.95:
                    Game1.createObjectDebris("(O)569", x, y, farmerId, location); // Granite
                    break;
                default:
                    Game1.createObjectDebris("(O)571", x, y, farmerId, location); // Limestone
                    break;
            }
        }

        internal static bool isFarmerMason(Farmer who)
        {
            return ModEntry.Config.EnableDogPowers
                && who.modData.TryGetValue($"{ModEntry.ModManifest.UniqueID}/ExtraMastery/Mason", out string value)
                && bool.Parse(value);
        }
    }
}
