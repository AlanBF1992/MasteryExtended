
using HarmonyLib;
using MasteryExtended.Menu.Pages;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Reflection;
using System.Reflection.Emit;

namespace MasteryExtended.Compatibility.VPP.Patches
{
    internal static class FarmerPatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        internal static IEnumerable<CodeInstruction> gainExperienceTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            try
            {
                CodeMatcher matcher = new(instructions, generator);

                MethodInfo expShareInfo = AccessTools.Method(typeof(FarmerPatch), nameof(expShare));
                MethodInfo currentLevelInfo = AccessTools.Method(typeof(FarmerPatch), nameof(currentLevel));

                // add:   if (currentSkillLevel(which) >= 10 && currentSkillLevel(which) < 20) expShare(who, which, howMuch); return;
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Pop),
                        new CodeMatch(OpCodes.Ldarg_0),
                        new CodeMatch(OpCodes.Ldfld)
                    )
                    .ThrowIfNotMatch("VPP FarmerGainExperiencePrefixTranspiler: IL code 3 not found")
                ;

                // ldarg_0 = this (Farmer)
                // ldarg_1 = which (int)
                // ldarg_2 = howMuch (int)

                matcher
                    .Advance(1)
                    .SetOpcodeAndAdvance(OpCodes.Ldarg_1)
                    .Insert(
                           new CodeInstruction(OpCodes.Ldarg_0)
                    )
                    .CreateLabel(out Label baseExpLbl)
                    .Insert(
                        //if
                        new CodeInstruction(OpCodes.Call, currentLevelInfo),
                        new CodeInstruction(OpCodes.Ldc_I4_S, 10),
                        new CodeInstruction(OpCodes.Blt_S, baseExpLbl),
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Call, currentLevelInfo),
                        new CodeInstruction(OpCodes.Ldc_I4_S, 20),
                        new CodeInstruction(OpCodes.Bge_S, baseExpLbl),
                        //código
                        new CodeInstruction(OpCodes.Ldarg_0),
                        new CodeInstruction(OpCodes.Ldarg_1),
                        new CodeInstruction(OpCodes.Ldarg_2),
                        new CodeInstruction(OpCodes.Call, expShareInfo),
                        //get me the fuck out
                        new CodeInstruction(OpCodes.Ret)
                    )
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(gainExperienceTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        private static int currentLevel(int which)
        {
            return MasterySkillsPage.skills.Find(s => s.Id == which)!.getLevel();
        }

        private static void expShare(Farmer who, int which, int howMuch)
        {
            int p = Math.Clamp(ModEntry.Config.MasteryPercentage, 0, 100);

            int expToSkill = (int)Math.Ceiling(howMuch * (100 - p) / 100.0);
            int expToMastery = howMuch - expToSkill;
            expToMastery = MasteryExtended.Patches.FarmerPatch.newMasteryAmount(expToMastery, which);

            if (which == 0 && expToMastery > 0)
            {
                expToMastery = Math.Max(1, expToMastery/2);
            }

            int currentMasteryLevel = MasteryTrackerMenu.getCurrentMasteryLevel();
            Game1.stats.Increment("MasteryExp", expToMastery);
            if (MasteryTrackerMenu.getCurrentMasteryLevel() > currentMasteryLevel)
            {
                Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:Mastery_newlevel"));
                Game1.playSound("newArtifact");
            }

            int newLevel = Farmer.checkForLevelGain(who.experiencePoints[which], who.experiencePoints[which] + expToSkill);
            who.experiencePoints[which] += expToSkill;
            int currentLevel = -1;
            if (newLevel != -1)
            {
                switch (which)
                {
                    case 0:
                        currentLevel = who.farmingLevel.Value;
                        who.farmingLevel.Value = newLevel;
                        break;
                    case 3:
                        currentLevel = who.miningLevel.Value;
                        who.miningLevel.Value = newLevel;
                        break;
                    case 1:
                        currentLevel = who.fishingLevel.Value;
                        who.fishingLevel.Value = newLevel;
                        break;
                    case 2:
                        currentLevel = who.foragingLevel.Value;
                        who.foragingLevel.Value = newLevel;
                        break;
                    case 5:
                        currentLevel = who.luckLevel.Value;
                        who.luckLevel.Value = newLevel;
                        break;
                    case 4:
                        currentLevel = who.combatLevel.Value;
                        who.combatLevel.Value = newLevel;
                        break;
                }
            }

            if (newLevel <= currentLevel)
            {
                return;
            }

            for (int i = currentLevel + 1; i <= newLevel; i++)
            {
                who.newLevels.Add(new Point(which, i));
                if (who.newLevels.Count == 1)
                {
                    Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:NewIdeas"));
                }
            }
        }

        // Actually from this mod. Makes it so the full Mastery Exp is gained at lvl 20
        internal static IEnumerable<CodeInstruction> ShouldGainMasteryExpTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);

                for (int i = 0; i < 5; i++)
                {
                    //from:  >= 10
                    //to:    >= MasteryCaveChanges()
                    matcher
                        .MatchEndForward(
                            new CodeMatch(OpCodes.Ldarg_0),
                            new CodeMatch(OpCodes.Ldfld),
                            new CodeMatch(OpCodes.Callvirt),
                            new CodeMatch(OpCodes.Ldc_I4_S),
                            new CodeMatch(OpCodes.Clt)

                        )
                        .ThrowIfNotMatch("Something Really Bad Happened: " + i)
                        .Advance(-1)
                        .Operand = 20;
                }

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(ShouldGainMasteryExpTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }
    }
}