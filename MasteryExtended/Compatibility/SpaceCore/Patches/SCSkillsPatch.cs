using HarmonyLib;
using MasteryExtended.Menu.Pages;
using MasteryExtended.Patches;
using MasteryExtended.Skills;
using MasteryExtended.Skills.Professions;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System.Reflection;
using System.Reflection.Emit;

namespace MasteryExtended.Compatibility.SpaceCore.Patches
{
    internal static class SCSkillsPatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        private static readonly IEnumerable<string> allSkillAdded = [];
        private static bool checkCookingSkills = true;

        internal static IEnumerable<CodeInstruction> AddExperienceTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);

                MethodInfo newMasteryAmountInfo = AccessTools.Method(typeof(FarmerPatch), nameof(FarmerPatch.newMasteryAmount));

                //from: if (prevLevel >= 10 && level >= 25)
                //to:   if (prevLevel >= 10)
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldloc_1),
                        new CodeMatch(OpCodes.Ldc_I4_S),
                        new CodeMatch(OpCodes.Blt_S)
                    )
                    .ThrowIfNotMatch("SCSkillsPatch.AddExperienceTranspiler: IL code 1 not found")
                    .RemoveInstructions(3)
                ;

                matcher
                    .MatchStartForward(
                            new CodeMatch(OpCodes.Ldarg_2)
                    )
                    .ThrowIfNotMatch("SCSkillsPatch.AddExperienceTranspiler: IL code 2 not found")
                    .Advance(1)
                    .Insert(
                        new CodeInstruction(OpCodes.Ldc_I4_5), // >= 5 doesn't give extra, only the 20%
                        new CodeInstruction(OpCodes.Call, newMasteryAmountInfo)
                    )
                ;


                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(AddExperienceTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        internal static void RegisterSkillPostfix(dynamic skill)
        {
            var getSkillLvl = AccessTools.Method("SpaceCore.Skills:GetSkillLevel");
            var newLevels = (List<KeyValuePair<string, int>>)AccessTools.Property("SpaceCore.Skills:NewLevels").GetValue(null)!;
            IEnumerable<dynamic> actualSCProfessions = ((IEnumerable<dynamic>)skill.ProfessionsForLevels)!.OrderBy(p => p.Level);
            List<Profession> myProfessions = [];
            foreach (var i in actualSCProfessions)
            {
                Profession? required = i.Level == 5 ? null : myProfessions.Find(p => p.Id == i.Requires.GetVanillaId());
                Profession first = new(i.First.GetVanillaId(),
                                       (Func<string>)(() => i.First.GetName()),
                                       (Func<string>)(() => i.First.GetDescription()),
                                       i.Level,
                                       required,
                                       (Func<Texture2D>)(() => i.First.Icon));
                Profession second = new(i.Second.GetVanillaId(),
                                        (Func<string>)(() => i.Second.GetName()),
                                        (Func<string>)(() => i.Second.GetDescription()),
                                        i.Level,
                                        required,
                                        (Func<Texture2D>)(() => i.Second.Icon));

                myProfessions.Add(first);
                myProfessions.Add(second);
            }

            Skill newSkill = new(() => skill.GetName(),
                              MasterySkillsPage.skills.Count,
                              (Texture2D?)skill.Icon,
                              myProfessions,
                              () => (int)getSkillLvl.Invoke(null, [Game1.player, skill.Id])!,
                              (lvl) => newLevels.Add(new KeyValuePair<string, int>(skill.Id, lvl)),
                              () => skill.ShouldShowOnSkillsPage,
                              [5, 10]);

            MasterySkillsPage.skills.Add(newSkill);
            allSkillAdded.AddItem((string)skill.Id);
            ModEntry.MaxMasteryLevels += 4;
            ModEntry.SkillsAvailable++;

            if (checkCookingSkills && allSkillAdded.Contains("moonslime.Cooking") && allSkillAdded.Contains("blueberry.LoveOfCooking.CookingSkill"))
            {
                checkCookingSkills = false;
                ModEntry.SkillsAvailable--;
                ModEntry.MaxMasteryLevels -= 4;
            }
        }
    }
}
