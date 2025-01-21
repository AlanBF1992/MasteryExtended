using StardewValley;
using DaLion.Professions.Framework;
using System.Reflection;
using HarmonyLib;
using System.Reflection.Emit;

namespace MasteryExtended.WoL.Patches
{
    internal static class LevelUpMenuUpdatePatch
    {
        internal static IEnumerable<CodeInstruction> LevelUpMenuUpdatePrefixTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            try
            {
                CodeMatcher matcher = new(instructions, generator);

                MethodInfo addRangeInfo = AccessTools.Method(typeof(LevelUpMenuUpdatePatch), nameof(lvl15AddRange));
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
                    .Advance(2)
                    .RemoveInstruction()
                    .Advance(1)
                    .RemoveInstructions(6)
                    .Insert(
                        new CodeInstruction(OpCodes.Call, addRangeInfo)
                    )
                ;

                // Lvl 20 (1)
                //from: int rootId = player.GetCurrentRootProfessionForSkill(skill);
                //to:   int rootId = lvl20Root(player, skill);
                matcher.
                    MatchStartForward(
                        new CodeMatch(OpCodes.Ldloc_0),
                        new CodeMatch(OpCodes.Ldloc_3),
                        new CodeMatch(OpCodes.Call),
                        new CodeMatch(OpCodes.Stloc_S)
                    )
                    .ThrowIfNotMatch("WoL LevelUpMenu: IL Code 1b not found")
                    .Advance(2)
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
                    .Advance(2)
                    .RemoveInstruction()
                    .Advance(1)
                    .RemoveInstructions(6)
                    .Insert(
                        new CodeInstruction(OpCodes.Call, addRangeInfo)
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
                    .Advance(2)
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
                    .Insert(
                        new CodeInstruction(OpCodes.Ldloc_0), //player
                        new CodeInstruction(OpCodes.Ldloc_3), //skill
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
                    .Insert(
                        new CodeInstruction(OpCodes.Ldloc_0), //player
                        new CodeInstruction(OpCodes.Ldloc_3), //skill
                        new CodeInstruction(OpCodes.Call, checkRoot20Info),
                        new CodeInstruction(OpCodes.Stloc_S, 19)
                    )
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception)
            {
                return instructions;
            }
        }

        internal static void lvl15AddRange(List<int> professionsToChoose, ISkill skill, Farmer player)
        {
            professionsToChoose.AddRange(skill.TierOneProfessions.Where(p => p.GetBranchingProfessions.Any(x => player.professions.Contains(x.Id))).Select(x => x.Id));
        }

        internal static int lvl15Root(Farmer player, ISkill skill)
        {
            return skill.TierOneProfessions.Where(p => p.GetBranchingProfessions.Any(x => player.professions.Contains(x.Id))).Select(x => x.Id).First();
        }

        internal static int lvl20Root(Farmer player, ISkill skill)
        {
            return skill.TierOneProfessions.Where(p => player.professions.Contains(p.Id + 100)).Select(x => x.Id).First();
        }
    }
}
