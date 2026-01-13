using HarmonyLib;
using MasteryExtended.Compatibility.GMCM;
using MasteryExtended.Patches;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace MasteryExtended
{
    internal static class VanillaLoader
    {
        internal static void Loader(IModHelper helper, Harmony harmony)
        {
            VanillaPatches(harmony);

            helper.Events.GameLoop.UpdateTicked += GMCMConfigVanilla;
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

            /***********************
             * Mastery Base Working
             ***********************/
            configMenu.AddSectionTitle(
                mod: ModEntry.ModManifest,
                text: () => "Exp per Level and Cave Access Settings"
            );

            // Show Skill Title on Hover of Skill Page
            configMenu.AddBoolOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.SkillNameOnMenuHover,
                setValue: (value) => ModEntry.Config.SkillNameOnMenuHover = value,
                name: () => "Skill Name on Menu Hover",
                tooltip: () => "Default: true"
            );

            // Show Profession Title on Hover of Skill Page
            configMenu.AddBoolOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.ProfessionNameOnMenuHover,
                setValue: (value) => ModEntry.Config.ProfessionNameOnMenuHover = value,
                name: () => "Profession Name on Menu Hover",
                tooltip: () => "Default: false"
            );

            // Mastery Experience per level
            configMenu.AddNumberOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.MasteryExpPerLevel,
                setValue: (value) => ModEntry.Config.MasteryExpPerLevel = value,
                name: () => "Mastery Experience for Level",
                tooltip: () => "Default: 30000",
                min: 15000,
                max: 50000,
                interval: 5000
            );

            // Which way the Cave unlocks
            configMenu.AddTextOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.SkillsVsMasteryPoints,
                setValue: value => ModEntry.Config.SkillsVsMasteryPoints = value,
                name: () => "How to access the Mastery Cave",
                tooltip: () => "What is required to access the cave.\nDefault: \"Level 10 Skills or Mastery Points\"",
                allowedValues: ["Level 10 Skills or Mastery Points", "Only Level 10 Skills", "Only Mastery Points", "Level 10 Skills AND Mastery Points"]
            );

            // Level 10 Skills Required for Cave
            configMenu.AddNumberOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.SkillsRequiredForMasteryRoom,
                setValue: (value) => ModEntry.Config.SkillsRequiredForMasteryRoom = value,
                name: () => "Skills ",
                tooltip: () => "Does nothing if \"Only Mastery Points\" is selected.\nDefault: 5",
                min: 0,
                max: ModEntry.SkillsAvailable,
                interval: 1
            );

            // Include Custom Skills on the count?
            configMenu.AddBoolOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.IncludeCustomSkills,
                setValue: (value) => ModEntry.Config.IncludeCustomSkills = value,
                name: () => "Count Custom Skills?",
                tooltip: () => "Does nothing if \"Only Mastery Points\" is selected.\nDefault: True"
            );

            // Mastery Required for Cave
            configMenu.AddNumberOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.MasteryRequiredForCave,
                setValue: (value) => ModEntry.Config.MasteryRequiredForCave = value,
                name: () => "Mastery Levels for the cave",
                tooltip: () => "Default: 5",
                min: 0,
                max: 10,
                interval: 1
            );

            /*****************************
             * Mastery Cave Inner Working
             *****************************/
            configMenu.AddSectionTitle(
                mod: ModEntry.ModManifest,
                text: () => "Mastery Cave Pillars and Pedestal Config"
            );

            // Which way the Pillars unlock
            configMenu.AddTextOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.PillarsVsProfessions,
                setValue: value => ModEntry.Config.PillarsVsProfessions = value,
                name: () => "Unlock order in Mastery Cave",
                tooltip: () => "What is required to unlock what.\nDefault: \"Professions required for Pillars\"",
                allowedValues: ["Professions required for Pillars", "Pillars required for Pedestal", "Neither"]
            );

            // Required Professions per pillar
            configMenu.AddNumberOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.RequiredProfessionForPillars,
                setValue: (value) => ModEntry.Config.RequiredProfessionForPillars = value,
                name: () => "Professions required for pillars",
                tooltip: () => "Does nothing if \"Professions required for Pillars\" is not selected.\nDefault: 3",
                min: 3,
                max: 6,
                interval: 1
            );

            // Required Pillars for Investment
            configMenu.AddNumberOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.RequiredPilarsToThePedestal,
                setValue: (value) => ModEntry.Config.RequiredPilarsToThePedestal = value,
                name: () => "Pillars required to invest",
                tooltip: () => "Does nothing if \"Pillars required for Pedestal\" is not selected.\nDefault: 3",
                min: 1,
                max: 5,
                interval: 1
            );

            // Confirm Acquisition of Profession
            configMenu.AddBoolOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.ConfirmProfession,
                setValue: (value) => ModEntry.Config.ConfirmProfession = value,
                name: () => "Confirm profession adquisition",
                tooltip: () => "Default: true"
            );
        }
    }
}
