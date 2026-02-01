using HarmonyLib;
using MasteryExtended.Compatibility.GMCM;
using MasteryExtended.Patches;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Constants;
using StardewValley.Menus;

namespace MasteryExtended
{
    internal static class VanillaLoader
    {
        internal static void Loader(IModHelper helper, Harmony harmony)
        {
            VanillaPatches(harmony);

            helper.Events.GameLoop.UpdateTicked += GMCMConfigVanilla;
            helper.Events.GameLoop.SaveLoaded += CalculatePillarsUnlocked;
        }

        /// <summary>Base patches for the mod.</summary>
        /// <param name="harmony">Harmony instance used to patch the game.</param>
        private static void VanillaPatches(Harmony harmony)
        {
            #region Experience and Mastery Gain
            /************************************
             * Farmer Experience & Mastery Gain *
             ************************************/
            // Cambia la forma en la que se calcula el nivel en Vanilla
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), "get_Level"),
                transpiler: new HarmonyMethod(typeof(FarmerPatch), nameof(FarmerPatch.LevelTranspiler))
            );

            // Cambia la forma en la que se entrega la info para el título
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.getTitle)),
                transpiler: new HarmonyMethod(typeof(FarmerPatch), nameof(FarmerPatch.getTitleTranspiler))
            );

            // Aumenta el máximo nivel de Mastery
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.getCurrentMasteryLevel)),
                postfix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.getCurrentMasteryLevelPostfix))
            );

            // Devuelve la experiencia necesaria para un nuevo nivel de maestría
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.getMasteryExpNeededForLevel)),
                prefix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.getMasteryExpNeededForLevelPrefix))
            );

            // Permite ganar Mastery desde que se llega al máximo de la primera profesión
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.gainExperience)),
                transpiler: new HarmonyMethod(typeof(FarmerPatch), nameof(FarmerPatch.gainExperienceTranspiler))
            );
            #endregion

            #region Mastery Bar and Numbers Drawing
            /*************************
             * Mastery Bar & Numbers *
             *************************/
            // Cambia el ancho mostrado por la barra de maestría y modifica los números
            harmony.Patch(
                original: AccessTools.Method(typeof(SkillsPage), nameof(SkillsPage.draw), [typeof(SpriteBatch)]),
                transpiler: new HarmonyMethod(typeof(SkillsPagePatch), nameof(SkillsPagePatch.drawTranspiler))
            );

            // Cambia el límite de los niveles de maestría.
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.drawBar)),
                transpiler: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.drawBarTranspiler))
            );
            #endregion

            #region Mastery Cave Map Changes
            /****************
             * Mastery Cave *
             ****************/
            // Al hacer clic en la puerta, te permite acceder antes y te dice como
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction), [typeof(string[]), typeof(Farmer), typeof(xTile.Dimensions.Location)]),
                transpiler: new HarmonyMethod(typeof(GameLocationPatch), nameof(GameLocationPatch.performActionTranspiler))
            );

            // Update the Mastery Cave map when needed
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.MakeMapModifications)),
                postfix: new HarmonyMethod(typeof(GameLocationPatch), nameof(GameLocationPatch.MakeMapModificationsPostfix))
            );
            #endregion

            #region Mastery Cave Pedestal and Pillars
            /**********************************
             * Mastery Cave Pedestal & Pillars
             **********************************/
            // Modifica el menú del pedestal. Lo hace más alto y crea un botón
            harmony.Patch(
                original: AccessTools.Constructor(typeof(MasteryTrackerMenu), [typeof(int)]),
                postfix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.ctorPostfix))
            );

            // Dibuja el botón
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.draw), [typeof(SpriteBatch)]),
                postfix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.drawPostfix))
            );

            // Le da funcionalidad al botón
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.receiveLeftClick)),
                prefix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.receiveLeftClickPrefix))
            );

            // Permite que el botón tenga "animación".
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.update)),
                prefix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.updatePrefix))
            );

            // Permite que el botón muestre el porqué no se puede aclamar
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.performHoverAction)),
                postfix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.performHoverActionPostfix))
            );

            // Bloquea el "arreglo" a la falta de profesión de nivel 10
            harmony.Patch(
                original: AccessTools.Method(typeof(LevelUpMenu), nameof(LevelUpMenu.AddMissedProfessionChoices)),
                prefix: new HarmonyMethod(typeof(LevelUpMenuPatch), nameof(LevelUpMenuPatch.AddMissedProfessionChoicesPrefix))
            );
            #endregion

            #region Show Acquired Professions
            harmony.Patch(
                original: AccessTools.Constructor(typeof(SkillsPage), [typeof(int), typeof(int), typeof(int), typeof(int)]),
                transpiler: new HarmonyMethod(typeof(SkillsPagePatch), nameof(SkillsPagePatch.ctorTranspiler))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(SkillsPage), nameof(SkillsPage.performHoverAction)),
                transpiler: new HarmonyMethod(typeof(SkillsPagePatch), nameof(SkillsPagePatch.performHoverActionTranspiler))
            );
            #endregion

            #region Others
            /***************
             * Dog Statue
             ***************/
            // Profession Forget Event
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
                postfix: new HarmonyMethod(typeof(GameLocationPatch), nameof(GameLocationPatch.answerDialogueActionPostFix))
            );
            #endregion
        }

        /// <summary>GMCM Compat Vanilla</summary>
        private static void GMCMConfigVanilla(object? _1, UpdateTickedEventArgs e)
        {
            if (e.Ticks < 10) return;
            ModEntry.ModHelper.Events.GameLoop.UpdateTicked -= GMCMConfigVanilla;
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = ModEntry.ModHelper.ModRegistry.GetApi<IGMCMApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;
            // register mod
            configMenu.Register(
                mod: ModEntry.ModManifest,
                reset: () => ModEntry.Config = new ModConfig(),
                save: () => ModEntry.ModHelper.WriteConfig(ModEntry.Config)
            );

            /******************
             * Basic Settings *
             ******************/
            configMenu.AddSectionTitle(
                mod: ModEntry.ModManifest,
                text: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_BasicSettingsTitle")
            );

            // Mastery Experience per level
            configMenu.AddNumberOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.MasteryExpPerLevel,
                setValue: (value) => ModEntry.Config.MasteryExpPerLevel = value,
                name: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_MasteryExperienceName"),
                tooltip: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_MasteryExperienceTooltip"),
                min: 15000,
                max: 50000,
                interval: 5000
            );

            /***********************
             * Skill Menu Settings *
             ***********************/
            configMenu.AddSectionTitle(
                mod: ModEntry.ModManifest,
                text: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_SkillMenuTitle")
            );

            // Show Skill Title on Hover of Skill Page
            configMenu.AddBoolOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.SkillNameOnMenuHover,
                setValue: (value) => ModEntry.Config.SkillNameOnMenuHover = value,
                name: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_SkillNameHoverName"),
                tooltip: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_SkillNameHoverTooltip")
            );

            // Show Profession Title on Hover of Skill Page
            configMenu.AddBoolOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.ProfessionNameOnMenuHover,
                setValue: (value) => ModEntry.Config.ProfessionNameOnMenuHover = value,
                name: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_ProfessionNameHoverName"),
                tooltip: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_ProfessionNameHoverTooltip")
            );


            /************************
             * Cave Access Settings *
             ************************/
            configMenu.AddSectionTitle(
                mod: ModEntry.ModManifest,
                text: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_CaveAccessSettingsTitle")
            );

            // Which way the Cave unlocks
            configMenu.AddTextOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.SkillsVsMasteryPoints,
                setValue: value => ModEntry.Config.SkillsVsMasteryPoints = value,
                name: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_HowToAccessCaveName"),
                tooltip: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_HowToAccessCaveTooltip", Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_HowToAccessCave0")),
                allowedValues: ["0", "1", "2", "3"],
                formatAllowedValue: (value) =>
                {
                    switch (value)
                    {
                        case "0":
                        case "1":
                        case "2":
                        case "3":
                            return Game1.content.LoadString($"Strings\\UI:MasteryExtended_GMCM_HowToAccessCave{value}");
                        default:
                            return Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_HowToAccessCave?");
                    }

                });


            // Include Custom Skills on the count?
            configMenu.AddBoolOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.IncludeCustomSkills,
                setValue: (value) => ModEntry.Config.IncludeCustomSkills = value,
                name: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_CustomSkillsForCaveName"),
                tooltip: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_CustomSkillsForCaveTooltip", Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_HowToAccessCave2"))
            );

            // Level 10 Skills Required for Cave
            configMenu.AddNumberOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.SkillsRequiredForMasteryRoom,
                setValue: (value) => ModEntry.Config.SkillsRequiredForMasteryRoom = value,
                name: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_SkillsRequiredForCaveName"),
                tooltip: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_SkillsRequiredForCaveTooltip", Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_HowToAccessCave2")),
                min: 0,
                max: ModEntry.SkillsAvailable,
                interval: 1
            );

            // Mastery Required for Cave
            configMenu.AddNumberOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.MasteryRequiredForCave,
                setValue: (value) => ModEntry.Config.MasteryRequiredForCave = value,
                name: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_MasteryRequiredForCaveName"),
                tooltip: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_MasteryRequiredForCaveTooltip", Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_HowToAccessCave1")),
                min: 0,
                max: 10,
                interval: 1
            );

            /*****************************
             * Mastery Cave Inner Working
             *****************************/
            configMenu.AddSectionTitle(
                mod: ModEntry.ModManifest,
                text: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_CavePillarsAndPedestalSettingsTitle")
            );

            // Which way the Pillars unlock
            configMenu.AddTextOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.PillarsVsProfessions,
                setValue: value => ModEntry.Config.PillarsVsProfessions = value,
                name: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_UnlockOrderCaveName"),
                tooltip: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_UnlockOrderCaveTooltip", Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_UnlockOrderCave0")),
                allowedValues: ["0", "1", "2"],
                formatAllowedValue: (value) =>
                {
                    switch (value)
                    {
                        case "0":
                        case "1":
                        case "2":
                            return Game1.content.LoadString($"Strings\\UI:MasteryExtended_GMCM_UnlockOrderCave{value}");
                        default:
                            return Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_UnlockOrderCave0?");
                    }
                }
            );

            // Required Professions per pillar
            configMenu.AddNumberOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.RequiredProfessionForPillars,
                setValue: (value) => ModEntry.Config.RequiredProfessionForPillars = value,
                name: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_ProfessionsRequiredForPillarsName"),
                tooltip: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_ProfessionsRequiredForPillarsTooltip", Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_UnlockOrderCave0")),
                min: 3,
                max: 6,
                interval: 1
            );

            // Required Pillars for Investment
            configMenu.AddNumberOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.RequiredPilarsToThePedestal,
                setValue: (value) => ModEntry.Config.RequiredPilarsToThePedestal = value,
                name: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_PillarsRequiredForProfessionsName"),
                tooltip: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_PillarsRequiredForProfessionsTooltip", Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_UnlockOrderCave1")),
                min: 1,
                max: 5,
                interval: 1
            );

            // Confirm Acquisition of Profession
            configMenu.AddBoolOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.ConfirmProfession,
                setValue: (value) => ModEntry.Config.ConfirmProfession = value,
                name: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_ConfirmProfessionAdquisitionName"),
                tooltip: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_ConfirmProfessionAdquisitionTooltip")
            );
        }

        private static void CalculatePillarsUnlocked(object? _1, SaveLoadedEventArgs _2)
        {
            uint count = 0;
            for (int i = 0; i < 5; i++)
            {
                count += Game1.player.stats.Get(StatKeys.Mastery(i));
            }
            Game1.player.stats.Set("mastery_total_pillars", count);
        }
    }
}
