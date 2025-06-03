using HarmonyLib;
using MasteryExtended.Compatibility.SpaceCore.Patches;
using MasteryExtended.Menu.Pages;
using MasteryExtended.Skills;
using MasteryExtended.Skills.Professions;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace MasteryExtended.Compatibility.SpaceCore
{
    internal static class SCLoader
    {
        internal static void Loader(IModHelper helper, Harmony harmony)
        {
            SpaceCorePatches(harmony);

            helper.Events.GameLoop.SaveLoaded += LoadSpaceCoreSkills;
        }

        /// <summary>Base patches for the mod.</summary>
        /// <param name="harmony">Harmony instance used to patch the game.</param>
        internal static void SpaceCorePatches(Harmony harmony)
        {
            #region Experience and Mastery Gain
            /************************************
             * Farmer Experience & Mastery Gain *
             ************************************/
            // Cambia como se gana experiencia en SpaceCore
            harmony.Patch(
                original: AccessTools.Method("SpaceCore.Skills:AddExperience"),
                transpiler: new HarmonyMethod(typeof(SCSkillsPatch), nameof(SCSkillsPatch.AddExperienceTranspiler))
            );
            #endregion

            #region Mastery Bar and Numbers Drawing
            /*************
             * SkillPage *
             *************/
            // Cambia el formato de los skillBars, para no que no dibuje nada
            Type skillsPage = AccessTools.TypeByName("SpaceCore.Interface.NewSkillsPage");
            harmony.Patch(
                original: AccessTools.Constructor(skillsPage, [typeof(int), typeof(int), typeof(int), typeof(int)]),
                transpiler: new HarmonyMethod(typeof(SCNewSkillsPagePatch), nameof(SCNewSkillsPagePatch.ctorTranspiler))
            );

            // Cambia el ancho mostrado por la barra de maestría, modifica los números y agrega el nuevo dibujo de las profesiones
            harmony.Patch(
                original: AccessTools.Method("SpaceCore.Interface.NewSkillsPage:draw", [typeof(SpriteBatch)]),
                transpiler: new HarmonyMethod(typeof(SCNewSkillsPagePatch), nameof(SCNewSkillsPagePatch.drawTranspiler))
            );

            // Se asegura que se vean las profesiones al hacer Hover
            harmony.Patch(
                original: AccessTools.Method("SpaceCore.Interface.NewSkillsPage:performHoverAction"),
                transpiler: new HarmonyMethod(typeof(SCNewSkillsPagePatch), nameof(SCNewSkillsPagePatch.performHoverActionTranspiler))
            );
            #endregion
        }

        /// <summary>Load the SpaceCore Skills, in case they exist</summary>
        private static void LoadSpaceCoreSkills(object? sender, SaveLoadedEventArgs e)
        {
            // Skill Things
            var skillList = (string[])AccessTools.Method("SpaceCore.Skills:GetSkillList").Invoke(null, null)!;
            var getSkill = AccessTools.Method("SpaceCore.Skills:GetSkill");
            var getSkillLvl = AccessTools.Method("SpaceCore.Skills:GetSkillLevel");
            var newLevels = (List<KeyValuePair<string, int>>)AccessTools.Property("SpaceCore.Skills:NewLevels").GetValue(null)!;

            foreach (string id in skillList)
            {
                dynamic actualSCSkill = getSkill.Invoke(null, [id])!;
                IOrderedEnumerable<dynamic> actualSCProfessions = (actualSCSkill.ProfessionsForLevels as System.Collections.IList)!.Cast<dynamic>().OrderBy((dynamic p) => p.Level);
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

                Skill skill = new(() => actualSCSkill.GetName(),
                                  MasterySkillsPage.skills.Count,
                                  (Texture2D?)actualSCSkill.Icon,
                                  myProfessions,
                                  () => (int)getSkillLvl.Invoke(null, [Game1.player, id])!,
                                  (lvl) => newLevels.Add(new KeyValuePair<string, int>(actualSCSkill.Id, lvl)),
                                  () => actualSCSkill.ShouldShowOnSkillsPage);

                MasterySkillsPage.skills.Add(skill);
                ModEntry.MaxMasteryLevels += 4;
            }

            //Just run once
            ModEntry.ModHelper.Events.GameLoop.SaveLoaded -= LoadSpaceCoreSkills;
        }
    }
}