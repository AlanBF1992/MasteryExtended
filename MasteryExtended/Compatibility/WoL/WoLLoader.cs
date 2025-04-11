using HarmonyLib;
using MasteryExtended.Compatibility.GMCM;
using MasteryExtended.Compatibility.WoL.Patches;
using MasteryExtended.Menu.Pages;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
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
            helper.Events.GameLoop.SaveLoaded += reloadIcons;
            ModEntry.MaxMasteryLevels += 20;
        }

        /// <summary>Makes SpaceCore Skills only able to go to lvl 10</summary>
        private static void fixExperienceCurve(object? sender, SaveLoadedEventArgs e)
        {
            int[] ExperienceCurve =
                [
                    100,
                    380,
                    770,
                    1300,
                    2150,
                    3300,
                    4800,
                    6900,
                    10000,
                    15000,
                    int.MaxValue
                ];

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
        private static void reloadIcons(object? _1, SaveLoadedEventArgs _2)
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
                postfix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.MasteryTrackerMenuPostfix))
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
                postfix: new HarmonyMethod(typeof(MasteryProfessionsPagePatch), nameof(MasteryProfessionsPagePatch.MasteryProfessionsPagePatchPostfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryProfessionsPage), nameof(MasteryProfessionsPage.draw), [typeof(SpriteBatch)]),
                postfix: new HarmonyMethod(typeof(MasteryProfessionsPagePatch), nameof(MasteryProfessionsPagePatch.drawPostfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryProfessionsPage), nameof(MasteryProfessionsPage.receiveLeftClick)),
                postfix: new HarmonyMethod(typeof(MasteryProfessionsPagePatch), nameof(MasteryProfessionsPagePatch.receiveLeftClickPostfix))
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
        }
    }
}
