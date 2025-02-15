using HarmonyLib;
using MasteryExtended.Patches;
using MasteryExtended.Skills;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Menus;
using System.Reflection;
using System.Reflection.Emit;

namespace MasteryExtended.Compatibility.WoL.Patches
{
    internal static class GainExperiencePatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        internal static IEnumerable<CodeInstruction> FarmerGainExperiencePrefixTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            try
            {
                CodeMatcher matcher = new(instructions, generator);

                MethodInfo howMuchWhichInfo = AccessTools.Method(typeof(GainExperiencePatch), nameof(howMuchWhich));
                MethodInfo expShareInfo = AccessTools.Method(typeof(GainExperiencePatch), nameof(expShare));
                MethodInfo currentLevelInfo = AccessTools.Method("DaLion.Professions.Framework.VanillaSkill:get_CurrentLevel");
                MethodInfo canGainPrestigeInfo = AccessTools.Method("DaLion.Professions.Framework.VanillaSkill:CanGainPrestigeLevels");

                // from: if (((skill.CurrentLevel == 10 && !skill.CanGainPrestigeLevels()) || skill.CurrentLevel == 20) && Skill.List.All(s => s.CurrentLevel >= 10))
                // to:   if (((skill.CurrentLevel == 10 && !skill.CanGainPrestigeLevels()) || skill.CurrentLevel == 20))
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Call),
                        new CodeMatch(OpCodes.Brfalse_S),
                        new CodeMatch(OpCodes.Call),
                        new CodeMatch(OpCodes.Stloc_2)
                    )
                    .ThrowIfNotMatch("WoL FarmerGainExperiencePrefixTranspiler: IL code 1 not found")
                    .Advance(2)
                    .CreateLabel(out Label alwaysTrue)
                    .Advance(-1)
                    .Set(OpCodes.Brfalse_S, alwaysTrue)
                ;

                // from: Game1.stats.Increment("MasteryExp", howMuch);
                // to:   Game1.stats.Increment("MasteryExp", howMuchWhich(howMuch, which));
                matcher
                    .MatchEndForward(
                        new CodeMatch(OpCodes.Ldstr),
                        new CodeMatch(OpCodes.Ldarg_2)
                    )
                    .ThrowIfNotMatch("WoL FarmerGainExperiencePrefixTranspiler: IL code 2 not found")
                    .Advance(2)
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldarg_1),   //which
                        new CodeInstruction(OpCodes.Call, howMuchWhichInfo)
                    )
                ;

                // add:   if (currentSkillLevel >= 10 && canGainPrestigeLevels) expShare(skill, howMuch)
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldloc_0),
                        new CodeMatch(OpCodes.Callvirt),
                        new CodeMatch(OpCodes.Ldloc_0),
                        new CodeMatch(OpCodes.Callvirt)
                    )
                    .ThrowIfNotMatch("WoL FarmerGainExperiencePrefixTranspiler: IL code 3 not found")
                ;

                Label leaveLbl = (Label)matcher.Advance(-1).Operand;

                matcher
                    .Advance(2)
                    .Insert(
                           new CodeInstruction(OpCodes.Ldloc_0)
                    )
                    .CreateLabel(out Label baseExpLbl)
                    .Insert(
                        new CodeInstruction(OpCodes.Call, currentLevelInfo),
                        new CodeInstruction(OpCodes.Ldc_I4_S, 10),
                        new CodeInstruction(OpCodes.Blt_S, baseExpLbl),
                        new CodeInstruction(OpCodes.Ldloc_0),
                        new CodeInstruction(OpCodes.Call, canGainPrestigeInfo),
                        new CodeInstruction(OpCodes.Brfalse_S, baseExpLbl),
                        new CodeInstruction(OpCodes.Ldloc_0),
                        new CodeInstruction(OpCodes.Ldarg_2),
                        new CodeInstruction(OpCodes.Ldind_I4),
                        new CodeInstruction(OpCodes.Call, expShareInfo),
                        new CodeInstruction(OpCodes.Ldc_I4_0),
                        new CodeInstruction(OpCodes.Stloc_3),
                        new CodeInstruction(OpCodes.Leave_S, leaveLbl)
                    )
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(FarmerGainExperiencePrefixTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        /***********
         * Methods *
         ***********/
        private static int howMuchWhich(int howMuch, int which)
        {
            return Math.Max(1, which == 0 ? (int)(0.5 * howMuch) : howMuch);
        }

        private static void expShare(object skill, int howMuch)
        {
            var currentSkillLevel = (int)skill.InvokeFunction("get_CurrentLevel", [])!;
            var skillId = (int)skill.GetType().GetMethod("get_Id")!.Invoke(skill, [])!; //which
            var currentExp = (int)skill.GetType().GetMethod("get_CurrentExp")!.Invoke(skill, [])!; //which
            var maxLevel = (int)skill.GetType().GetMethod("get_MaxLevel")!.Invoke(skill, [])!; //which

            var addExperienceMethod = skill.GetType().GetMethod("AddExperience")!;
            var setLevelMethod = skill.GetType().GetMethod("SetLevel")!;

            int p = ModEntry.Config.MasteryPercentage;

            int expToSkill = (int)Math.Ceiling(howMuch * (100 - p) / 100.0);
            int expToMastery = (skillId != 0) ? (howMuch - expToSkill) : (int)((howMuch - expToSkill) / 2.0);

            // Add the level Exp
            var newLevel = Math.Min(Farmer.checkForLevelGain(currentExp, currentExp + expToSkill), maxLevel);
            var old = MasteryTrackerMenu.getCurrentMasteryLevel();

            addExperienceMethod.Invoke(skill, [expToSkill]);
            Game1.stats.Increment("MasteryExp", expToMastery);

            if (newLevel > currentSkillLevel)
            {
                setLevelMethod.Invoke(skill, [newLevel]);
                Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:NewIdeas"));
            }

            // Add the mastery Exp
            if (MasteryTrackerMenu.getCurrentMasteryLevel() > old)
            {
                Game1.showGlobalMessage(Game1.content.LoadString("Strings\\1_6_Strings:Mastery_newlevel"));
                Game1.playSound("newArtifact");
            }
        }
    }
}
