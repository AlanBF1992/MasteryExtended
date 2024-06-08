using StardewModdingAPI;
using StardewModdingAPI.Events;
using MasteryExtended.Menu.Pages;
using MasteryExtended.Skills.Professions;
using StardewValley;
using HarmonyLib;
using SpaceCore.Interface;
using MasteryExtended.SC.Patches;
using Microsoft.Xna.Framework.Graphics;
using Skill = MasteryExtended.Skills.Skill;

namespace MasteryExtended.SC
{
    public class ModEntry: Mod
    {
        /// <summary>Log info.</summary>
        public static IMonitor LogMonitor { get; private set; } = null!;
        public static IModHelper ModHelper { get; private set; } = null!;

        public bool SkillsLoaded { get; set; } = false;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            LogMonitor = Monitor;
            ModHelper = helper;

            // Patch SpaceCore
            var harmony = new Harmony(this.ModManifest.UniqueID);

            /*************
             * Experience
             *************/
            harmony.Patch(
                original: AccessTools.Method(typeof(SpaceCore.Skills), nameof(SpaceCore.Skills.AddExperience)),
                prefix: new HarmonyMethod(typeof(SkillsPatch), nameof(SkillsPatch.AddExperiencePrefix))
            );

            /************
             * SkillPage
             ************/
            harmony.Patch(
                original: AccessTools.Method(typeof(NewSkillsPage), nameof(NewSkillsPage.draw), new Type[] { typeof(SpriteBatch) }),
                prefix: new HarmonyMethod(typeof(NewSkillsPagePatch), nameof(NewSkillsPagePatch.drawPrefix))
            );
            harmony.Patch(
                original: AccessTools.Method(typeof(NewSkillsPage), nameof(NewSkillsPage.draw), new Type[] { typeof(SpriteBatch) }),
                postfix: new HarmonyMethod(typeof(NewSkillsPagePatch), nameof(NewSkillsPagePatch.drawPostfix))
            );

            //Events
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;

            //Console Commands
            helper.ConsoleCommands.Add(
                "masteryExtended_ResetProfessionsSpaceCore",
                "Reset SpaceCore Professions when you sleep.",
                (_, __) => { resetAllProfessionsSpaceCore(); MasteryExtended.ModEntry.recountUsedMasteryLevels(); });
        }

        [EventPriority(EventPriority.Normal)]
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            /***********************
             * Add SpaceCore Skills
             ***********************/
            // Done here because BirbCore
            if (SkillsLoaded) { return; }
            SkillsLoaded = true;
            foreach (string id in SpaceCore.Skills.GetSkillList())
            {
                SpaceCore.Skills.Skill actualSCSkill = SpaceCore.Skills.GetSkill(id);

                IOrderedEnumerable<SpaceCore.Skills.Skill.ProfessionPair> actualSCProfessions = actualSCSkill.ProfessionsForLevels.OrderBy(p => p.Level);

                List<Profession> myProfessions = new();

                foreach (var i in actualSCProfessions)
                {
                    Profession? myRequiredProfession = i.Level == 5 ? null : myProfessions.Find(p => p.Id == i.Requires.GetVanillaId());
                    Profession myProfessionFirst = new(i.First.GetVanillaId(), () => i.First.GetName(), i.Level, () => i.First.GetDescription(), i.First.Icon, null, myRequiredProfession);
                    Profession myProfessionSecond = new(i.Second.GetVanillaId(), () => i.Second.GetName(), i.Level, () => i.Second.GetDescription(), i.Second.Icon, null, myRequiredProfession);

                    myProfessions.Add(myProfessionFirst);
                    myProfessions.Add(myProfessionSecond);
                }

                Skill mySkill = new(() => actualSCSkill.GetName(), MasterySkillsPage.skills.Count, SpaceCore.Skills.GetSkillIcon(id), null, myProfessions)
                {
                    getLevel = () => SpaceCore.Skills.GetSkillLevel(Game1.player, id),
                    showSkill = () => actualSCSkill.ShouldShowOnSkillsPage
                };

                MasterySkillsPage.skills.Add(mySkill);
                MasteryExtended.ModEntry.MaxMasteryPoints += 4;
            }
        }

        private void resetAllProfessionsSpaceCore()
        {
            if (!Context.IsWorldReady)
            {
                LogMonitor.Log("You need to load a save to use this command.", LogLevel.Error);
                return;
            }

            Game1.player.professions.RemoveWhere(p => p < 0 || 29 < p);

            foreach (Skill s in MasterySkillsPage.skills.FindAll(s => s.Id < 0 || 4 < s.Id))
            {
                int level = s.getLevel();
                string skillName = SpaceCore.Skills.GetSkill(s.GetName()).Id;
                // SpaceCore NewLevels is the same as vanilla newLevels.
                var NewLevels = Helper.Reflection.GetField<List<KeyValuePair<string, int>>>(typeof(SpaceCore.Skills), "NewLevels");
                var currentNewLevels = NewLevels.GetValue();
                if (level >= 5)
                {
                    currentNewLevels.Add(new KeyValuePair<string, int>(skillName, 5));
                }
                if (level >= 10)
                {
                    currentNewLevels.Add(new KeyValuePair<string, int>(skillName, 10));
                }
                NewLevels.SetValue(currentNewLevels);
            }
        }
    }
}
