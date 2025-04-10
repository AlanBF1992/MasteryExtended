using HarmonyLib;
using MasteryExtended.Menu.Pages;
using StardewModdingAPI;
using StardewValley;
using System.Reflection;
using System.Reflection.Emit;

namespace MasteryExtended.Compatibility.WoL.Patches
{
    internal static class LevelUpMenuUpdatePatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        internal static IEnumerable<CodeInstruction> LevelUpMenuUpdatePrefixTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            try
            {
                CodeMatcher matcher = new(instructions, generator);

                MethodInfo lvl15AddRangeInfo = AccessTools.Method(typeof(LevelUpMenuUpdatePatch), nameof(lvl15AddRange));
                MethodInfo checkRoot15Info = AccessTools.Method(typeof(LevelUpMenuUpdatePatch), nameof(lvl15Root));
                MethodInfo checkRoot20Info = AccessTools.Method(typeof(LevelUpMenuUpdatePatch), nameof(lvl20Root));

                // Lvl 15 (1)
                //from: ___professionsToChoose.AddRange(skill.TierOneProfessionIds.Where(player.professions.Contains));
                //to:   lvl15AddRange(___professionsToChoose, skill, player)
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldarg_1),
                        new CodeMatch(OpCodes.Ldloc_3),
                        new CodeMatch(OpCodes.Callvirt),
                        new CodeMatch(OpCodes.Ldloc_0)
                    )
                    .ThrowIfNotMatch("WoL LevelUpMenu: IL Code 1a not found")
                    .Advance(1)
                    .Set(OpCodes.Ldarg_S, 5) // ldloc_3 => ldarg_s 5
                    .Advance(1)
                    .RemoveInstructions(8)
                    .Insert(
                        new CodeInstruction(OpCodes.Call, lvl15AddRangeInfo)
                    )
                ;

                // Lvl 20 (1)
                //from: int rootId = player.GetCurrentRootProfessionForSkill(skill);
                //to:   int rootId = lvl20Root(skillId);
                matcher.
                    MatchStartForward(
                        new CodeMatch(OpCodes.Ldloc_0),
                        new CodeMatch(OpCodes.Ldloc_3),
                        new CodeMatch(OpCodes.Call),
                        new CodeMatch(OpCodes.Stloc_S)
                    )
                    .ThrowIfNotMatch("WoL LevelUpMenu: IL Code 1b not found")
                    .Set(OpCodes.Ldarg_S, 5) // ldloc_0 => ldarg_s 5
                    .Advance(1)
                    .RemoveInstruction()
                    .Set(OpCodes.Call, checkRoot20Info)
                ;

                // Lvl 15 (2)
                //from: ___professionsToChoose.AddRange(skill.TierOneProfessionIds.Where(player.professions.Contains));
                //to:   lvl15AddRange(___professionsToChoose, skill, player)
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldarg_1),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Callvirt),
                        new CodeMatch(OpCodes.Ldloc_0)
                    )
                    .ThrowIfNotMatch("WoL LevelUpMenu: IL Code 2a not found")
                    .Advance(1)
                    .Set(OpCodes.Ldarg_S, 5) // Ldloc_S => Ldarg_S 5
                    .Advance(1)
                    .RemoveInstructions(8)
                    .Insert(
                        new CodeInstruction(OpCodes.Call, lvl15AddRangeInfo)
                    )
                ;

                // Lvl 20 (2)
                //from: int rootId = player.GetCurrentRootProfessionForSkill(skill);
                //to:   int rootId = lvl20Root(player, skill);
                matcher.
                    MatchStartForward(
                        new CodeMatch(OpCodes.Ldloc_0),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Call),
                        new CodeMatch(OpCodes.Stloc_S)
                    )
                    .ThrowIfNotMatch("WoL LevelUpMenu: IL Code 2b not found")
                    .Set(OpCodes.Ldarg_S, 5) // ldloc_0 => ldarg_s 5
                    .Advance(1)
                    .RemoveInstruction()
                    .Set(OpCodes.Call, checkRoot20Info)
                ;

                // Lvl 15 (3)
                //from: 
                //to:   rootId = lvl15Root(player, skill)
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldloc_0), //player
                        new CodeMatch(OpCodes.Ldfld),   //professions
                        new CodeMatch(OpCodes.Ldloc_S), //rootid
                        new CodeMatch(OpCodes.Ldc_I4_S) //100
                    )
                    .ThrowIfNotMatch("WoL LevelUpMenu: IL Code 3a not found")
                    .Advance(1)
                    .Insert(
                        new CodeInstruction(OpCodes.Ldarg_S, 5), //skillId
                        new CodeInstruction(OpCodes.Call, checkRoot15Info),
                        new CodeInstruction(OpCodes.Stloc_S, 19)
                    )
                ;

                // Lvl 20 (3)
                //from: 
                //to:   rootId = lvl20Root(player, skill)
                matcher.
                    MatchStartForward(
                        new CodeMatch(OpCodes.Ldloc_0),
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Call),
                        new CodeMatch(OpCodes.Call),
                        new CodeMatch(OpCodes.Stloc_S)
                    )
                    .ThrowIfNotMatch("WoL LevelUpMenu: IL Code 2b not found")
                    .Advance(1)
                    .Insert(
                        new CodeInstruction(OpCodes.Ldarg_S, 5), //skillId
                        new CodeInstruction(OpCodes.Call, checkRoot20Info),
                        new CodeInstruction(OpCodes.Stloc_S, 19)
                    )
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(LevelUpMenuUpdatePrefixTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        /***********
         * Methods *
         ***********/

        internal static void lvl15AddRange(List<int> professionsToChoose, int skillId)
        {
            var mySkill = MasterySkillsPage.skills.Find(s => s.Id == skillId)!;
            var unlockedLvl10Profs = mySkill.Professions.Where(p => p.LevelRequired == 10 && p.IsProfessionUnlocked());

            professionsToChoose.AddRange(unlockedLvl10Profs.Select(p => p.RequiredProfessions!.Id).Distinct());
        }

        /****************************
         * When only one profession *
         ****************************/
        internal static int lvl15Root(int skillId)
        {
            var skill = MasterySkillsPage.skills.Find(s => s.Id == skillId)!;
            var unlockedLvl10Profs = skill.Professions.Where(p => p.LevelRequired == 10 && p.IsProfessionUnlocked());

            return unlockedLvl10Profs.Select(p => p.RequiredProfessions!.Id).Distinct().First();
        }

        internal static int lvl20Root(int skillId)
        {
            var skill = MasterySkillsPage.skills.Find(s => s.Id == skillId)!;
            var prestigedLvl5Prof = skill.Professions.Find(p => Game1.player.professions.Contains(p.Id + 100))!.Id;

            return skill.Professions.Find(p => p.RequiredProfessions?.Id == prestigedLvl5Prof && p.IsProfessionUnlocked())!.Id;
        }
    }
}
