using HarmonyLib;
using MasteryExtended.Compatibility.GMCM;
using MasteryExtended.Compatibility.WoL.Patches;
using MasteryExtended.Menu.Pages;
using MasteryExtended.Skills.Professions;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using MasteryTrackerMenuPatch = MasteryExtended.Compatibility.WoL.Patches.MasteryTrackerMenuPatch;

namespace MasteryExtended.Compatibility.WoL
{
    internal static class WoLLoader
    {
        internal static void Loader(IModHelper helper, Harmony harmony)
        {
            WoLPatches(harmony);
            helper.Events.GameLoop.SaveLoaded += fixExperienceCurve;
            helper.Events.GameLoop.GameLaunched += GMCMConfigWoL;
            helper.Events.GameLoop.DayStarted += (_, _) => reloadIcons(); //Because fuck my life.
            ModEntry.MaxMasteryLevels += 20;
            foreach (var skill in MasterySkillsPage.skills.Where(s => s.Id >= 0 && s.Id <= 4))
            {
                skill.ProfessionChooserLevels.AddRange([15, 20]);
            }
        }

        /// <summary>Makes SpaceCore Skills only able to go to lvl 10</summary>
        private static void fixExperienceCurve(object? sender, SaveLoadedEventArgs e)
        {
            int[] ExperienceCurve = new int[11];

            for (int i = 0; i < 10; i++)
            {
                ExperienceCurve[i] = Farmer.getBaseExperienceForLevel(i + 1);
            }

            ExperienceCurve[10] = int.MaxValue;

            var skillList = (string[])AccessTools.Method("SpaceCore.Skills:GetSkillList").Invoke(null, null)!;
            var getSkill = AccessTools.Method("SpaceCore.Skills:GetSkill");

            foreach (string skill in skillList)
            {
                ((dynamic)getSkill.Invoke(null, [skill])!).ExperienceCurve = ExperienceCurve;
            }
        }

        /// <summary>GMCM Compat WoL</summary>
        private static void GMCMConfigWoL(object? _1, GameLaunchedEventArgs _2)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = ModEntry.ModHelper.ModRegistry.GetApi<IGMCMApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // Add Wol Section
            configMenu.AddSectionTitle(
                mod: ModEntry.ModManifest,
                text: () => "WoL Compat Options"
            );

            // Percentage of Exp to Mastery
            configMenu.AddNumberOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.MasteryPercentage,
                setValue: (value) => ModEntry.Config.MasteryPercentage = value,
                name: () => "Experience Percent for Mastery",
                tooltip: () => "Between Level 11 and 20. Default: 20",
                min: 0,
                max: 100,
                interval: 1
            );
        }

        /// <summary>Reload vanilla icons so WoL icons are correctly shown</summary>
        internal static void reloadIcons()
        {
            ModEntry.ModHelper.GameContent.InvalidateCache("LooseSprites/Cursors");
        }

        /// <summary>All the patches to make WoL work</summary>
        private static void WoLPatches(Harmony harmony)
        {
            /*************************************
             * Vanilla Skill Experience with WoL *
             *************************************/
            // Add Partial Mastery Exp when lvl 10 to 19 and other fixes
            harmony.Patch(
                original: AccessTools.Method("DaLion.Professions.Framework.Patchers.Prestige.FarmerGainExperiencePatcher:FarmerGainExperiencePrefix"),
                transpiler: new HarmonyMethod(typeof(GainExperiencePatch), nameof(GainExperiencePatch.FarmerGainExperiencePrefixTranspiler))
            );

            /********************
             * MAESTRÍAS CON WoL
             ********************/

            // No haga nada al cargar la partida
            harmony.Patch(
                original: AccessTools.Method("DaLion.Professions.Framework.Events.GameLoop.ProfessionSaveLoadedEvent:OnSaveLoadedImpl"),
                postfix: new HarmonyMethod(typeof(VanillaSkillPatch), nameof(VanillaSkillPatch.OnSaveLoadedImplPostfix))
            );

            /**************************
             * CAMBIAR DE COMBAT LIMIT
             **************************/

            // Accede al menú y reactiva el main button
            harmony.Patch(
                original: AccessTools.Constructor(typeof(MasteryTrackerMenu), [typeof(int)]),
                postfix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.ctorPostfix))
            );

            // Dibuja el botón
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.draw), [typeof(SpriteBatch)]),
                prefix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.drawPrefix)),
                postfix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.drawPostfix))
            );

            // Le da funcionalidad al botón
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.receiveLeftClick)),
                prefix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.receiveLeftClickPrefix))
            );

            // Highlight
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.performHoverAction)),
                postfix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.performHoverActionPostfix))
            );

            /***************
             * LVL 15 AL 20
             ***************/
            harmony.Patch(
                original: AccessTools.Constructor(typeof(MasteryProfessionsPage), [typeof(Skills.Skill)]),
                postfix: new HarmonyMethod(typeof(MasteryProfessionsPagePatch), nameof(MasteryProfessionsPagePatch.ctorPostfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryProfessionsPage), nameof(MasteryProfessionsPage.draw), [typeof(SpriteBatch)]),
                postfix: new HarmonyMethod(typeof(MasteryProfessionsPagePatch), nameof(MasteryProfessionsPagePatch.drawPostfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryProfessionsPage), nameof(MasteryProfessionsPage.receiveLeftClick)),
                prefix: new HarmonyMethod(typeof(MasteryProfessionsPagePatch), nameof(MasteryProfessionsPagePatch.receiveLeftClickPrefix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryProfessionsPage), nameof(MasteryProfessionsPage.performHoverAction)),
                postfix: new HarmonyMethod(typeof(MasteryProfessionsPagePatch), nameof(MasteryProfessionsPagePatch.performHoverActionPostfix))
            );

            harmony.Patch(
                original: AccessTools.Method("DaLion.Professions.Framework.Patchers.Prestige.LevelUpMenuUpdatePatcher:LevelUpMenuUpdatePrefix"),
                transpiler: new HarmonyMethod(typeof(LevelUpMenuUpdatePatch), nameof(LevelUpMenuUpdatePatch.LevelUpMenuUpdatePrefixTranspiler))
            );

            //For now SpaceCore skills can only go to level 10
            harmony.Patch(
                original: AccessTools.Method("DaLion.Professions.Framework.Patchers.Prestige.Integration.SkillLevelUpMenuUpdatePatcher:SkillLevelUpMenuUpdatePrefix"),
                prefix: new HarmonyMethod(typeof(DaLionUnpatcher), nameof(DaLionUnpatcher.UnpatcherBoolPrefix))
            );

            //Fix Message
            harmony.Patch(
                original: AccessTools.Method("DaLion.Professions.Framework.Patchers.Prestige.GameLocationPerformActionPatcher:GameLocationPerformActionPrefix"),
                prefix: new HarmonyMethod(typeof(DaLionUnpatcher), nameof(DaLionUnpatcher.UnpatcherBoolPrefix))
            );

            //Fix MasteryExtended placement?
            harmony.Patch(
                original: AccessTools.Method("DaLion.Professions.Framework.Patchers.Prestige.Integration.NewSkillsPagePerformHoverActionPatcher:NewSkillsPagePerformHoverActionPostfix"),
                prefix: new HarmonyMethod(typeof(DaLionUnpatcher), nameof(DaLionUnpatcher.UnpatcherVoidPrefix))
            );

            //Fix Warning when claiming mastery
            var masteryBox = AccessTools.TypeByName("DaLion.Professions.Framework.UI.MasteryWarningBox");
            harmony.Patch(
                original: AccessTools.Constructor(masteryBox, [typeof(GameLocation), typeof(MasteryTrackerMenu)]),
                transpiler: new HarmonyMethod(typeof(MasteryWarningBoxPatch), nameof(MasteryWarningBoxPatch.ctorTranspiler))
            );

            harmony.Patch(
                original: AccessTools.Method("DaLion.Professions.Framework.UI.MasteryWarningBox:draw", [typeof(SpriteBatch)]),
                transpiler: new HarmonyMethod(typeof(MasteryWarningBoxPatch), nameof(MasteryWarningBoxPatch.drawTranspiler))
            );

            //Force Reset Configs to be false
            harmony.Patch(
                original: AccessTools.PropertyGetter("DaLion.Professions.Framework.Configs.MasteriesConfig:LockMasteryUntilFullReset"),
                prefix: new HarmonyMethod(typeof(ConfigPatch), nameof(ConfigPatch.alwaysFalsePrefix))
            );

            harmony.Patch(
                original: AccessTools.PropertyGetter("DaLion.Professions.Framework.Configs.SkillsConfig:EnableSkillReset"),
                prefix: new HarmonyMethod(typeof(ConfigPatch), nameof(ConfigPatch.alwaysFalsePrefix))
            );

            // Remove prestiges correctly when removing professions
            harmony.Patch(
                original: AccessTools.Method(typeof(Profession), nameof(Profession.RemoveProfessionFromPlayer)),
                transpiler: new HarmonyMethod(typeof(ProfessionPatch), nameof(ProfessionPatch.RemoveProfessionFromPlayerTranspiler))
            );
        }
    }
}
