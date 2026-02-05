using HarmonyLib;
using MasteryExtended.Compatibility.GMCM;
using MasteryExtended.Patches;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Constants;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Powers;
using StardewValley.GameData.Shops;
using StardewValley.Menus;

namespace MasteryExtended
{
    internal static class VanillaLoader
    {
        internal static void Loader(IModHelper helper, Harmony harmony)
        {
            VanillaPatches(harmony);

            helper.Events.Content.AssetRequested += UIStringsAssetRequested;
            helper.Events.Content.AssetRequested += BookPowersAssetRequested;

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

            // Which way the Pillars unlock
            configMenu.AddTextOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.BooksQuantity,
                setValue: value => {
                    ModEntry.Config.BooksQuantity = value;
                    ModEntry.ModHelper.GameContent.InvalidateCache("Data\\Objects");
                    ModEntry.ModHelper.GameContent.InvalidateCache("Data\\Shops");
                    ModEntry.ModHelper.GameContent.InvalidateCache("Data\\Powers");
                },
                name: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_BookMasteryName"),
                tooltip: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_BookMasteryTooltip", Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_BookMasteryName0")),
                allowedValues: ["0", "1", "2"],
                formatAllowedValue: (value) =>
                {
                    return value switch
                    {
                        "0" or "1" or "2" => Game1.content.LoadString($"Strings\\UI:MasteryExtended_GMCM_BookMasteryName{value}"),
                        _ => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_BookMasteryName?"),
                    };
                    
                }
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
                    return value switch
                    {
                        "0" or "1" or "2" or "3" => Game1.content.LoadString($"Strings\\UI:MasteryExtended_GMCM_HowToAccessCave{value}"),
                        _ => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_HowToAccessCave?"),
                    };
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
                    return value switch
                    {
                        "0" or "1" or "2" => Game1.content.LoadString($"Strings\\UI:MasteryExtended_GMCM_UnlockOrderCave{value}"),
                        _ => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_UnlockOrderCave?"),
                    };
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

        /**********
         * ASSETS *
         **********/
        private static void UIStringsAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(PathUtilities.NormalizeAssetName("Strings/UI")))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsDictionary<string, string>();

                    editor.Data.Add("MasteryExtended_InvestButton", ModEntry.ModHelper.Translation.Get("invest-button"));
                    editor.Data.Add("MasteryExtended_BackButton", ModEntry.ModHelper.Translation.Get("back-button"));
                    editor.Data.Add("MasteryExtended_NextButton", ModEntry.ModHelper.Translation.Get("next-button"));
                    editor.Data.Add("MasteryExtended_MenuTitleSkills", ModEntry.ModHelper.Translation.Get("menu-title-skills"));
                    editor.Data.Add("MasteryExtended_MenuTitleProfession", ModEntry.ModHelper.Translation.Get("menu-title-profession"));
                    editor.Data.Add("MasteryExtended_HoverSkill", ModEntry.ModHelper.Translation.Get("hover-skill"));
                    editor.Data.Add("MasteryExtended_AddedProfession", ModEntry.ModHelper.Translation.Get("added-profession"));
                    editor.Data.Add("MasteryExtended_NeedMoreProfessions", ModEntry.ModHelper.Translation.Get("need-more-professions"));
                    editor.Data.Add("MasteryExtended_NeedMoreLevels", ModEntry.ModHelper.Translation.Get("need-more-levels"));
                    editor.Data.Add("MasteryExtended_CantSpend", ModEntry.ModHelper.Translation.Get("cant-spend"));
                    editor.Data.Add("MasteryExtended_LookOnly", ModEntry.ModHelper.Translation.Get("look-only"));
                    editor.Data.Add("MasteryExtended_CantAccessSkill", ModEntry.ModHelper.Translation.Get("cant-access-skill"));
                    editor.Data.Add("MasteryExtended_EveryProfessionUnlocked", ModEntry.ModHelper.Translation.Get("every-profession-unlocked"));
                    editor.Data.Add("MasteryExtended_TrascendMortalKnowledge", ModEntry.ModHelper.Translation.Get("transcend-mortal-knowledge"));
                    editor.Data.Add("MasteryExtended_TrascendMortalKnowledgeOnly", ModEntry.ModHelper.Translation.Get("transcend-mortal-knowledge-only"));
                    editor.Data.Add("MasteryExtended_TrascendMortalKnowledgeTogether", ModEntry.ModHelper.Translation.Get("transcend-mortal-knowledge-together"));
                    editor.Data.Add("MasteryExtended_AlreadyUnlocked", ModEntry.ModHelper.Translation.Get("already-unlocked"));
                    editor.Data.Add("MasteryExtended_RequirementsTitle", ModEntry.ModHelper.Translation.Get("requirements-title"));
                    editor.Data.Add("MasteryExtended_RequirementsProfession", ModEntry.ModHelper.Translation.Get("requirements-profession"));
                    editor.Data.Add("MasteryExtended_RequirementsLvl10", ModEntry.ModHelper.Translation.Get("requirements-lvl10"));
                    editor.Data.Add("MasteryExtended_RequirementsLvl15", ModEntry.ModHelper.Translation.Get("requirements-lvl15"));
                    editor.Data.Add("MasteryExtended_RequirementsLvl20", ModEntry.ModHelper.Translation.Get("requirements-lvl20"));
                    editor.Data.Add("MasteryExtended_WoLMasteryWarning", ModEntry.ModHelper.Translation.Get("wol-mastery-warning"));

                    editor.Data.Add("MasteryExtended_GMCM_BasicSettingsTitle", ModEntry.ModHelper.Translation.Get("gmcm-basic-settings-title"));
                    editor.Data.Add("MasteryExtended_GMCM_MasteryExperienceName", ModEntry.ModHelper.Translation.Get("gmcm-mastery-experience-name"));
                    editor.Data.Add("MasteryExtended_GMCM_MasteryExperienceTooltip", ModEntry.ModHelper.Translation.Get("gmcm-mastery-experience-tooltip"));
                    editor.Data.Add("MasteryExtended_GMCM_SkillMenuTitle", ModEntry.ModHelper.Translation.Get("gmcm-skill-menu-settings-title"));
                    editor.Data.Add("MasteryExtended_GMCM_SkillNameHoverName", ModEntry.ModHelper.Translation.Get("gmcm-skill-name-on-hover-name"));
                    editor.Data.Add("MasteryExtended_GMCM_SkillNameHoverTooltip", ModEntry.ModHelper.Translation.Get("gmcm-skill-name-on-hover-tooltip"));
                    editor.Data.Add("MasteryExtended_GMCM_ProfessionNameHoverName", ModEntry.ModHelper.Translation.Get("gmcm-profession-name-on-hover-name"));
                    editor.Data.Add("MasteryExtended_GMCM_ProfessionNameHoverTooltip", ModEntry.ModHelper.Translation.Get("gmcm-profession-name-on-hover-tooltip"));
                    editor.Data.Add("MasteryExtended_GMCM_CaveAccessSettingsTitle", ModEntry.ModHelper.Translation.Get("gmcm-cave-access-settings-title"));
                    editor.Data.Add("MasteryExtended_GMCM_HowToAccessCaveName", ModEntry.ModHelper.Translation.Get("gmcm-how-to-access-cave-name"));
                    editor.Data.Add("MasteryExtended_GMCM_HowToAccessCaveTooltip", ModEntry.ModHelper.Translation.Get("gmcm-how-to-access-cave-tooltip"));
                    editor.Data.Add("MasteryExtended_GMCM_HowToAccessCave0", ModEntry.ModHelper.Translation.Get("gmcm-how-to-access-cave-0"));
                    editor.Data.Add("MasteryExtended_GMCM_HowToAccessCave1", ModEntry.ModHelper.Translation.Get("gmcm-how-to-access-cave-1"));
                    editor.Data.Add("MasteryExtended_GMCM_HowToAccessCave2", ModEntry.ModHelper.Translation.Get("gmcm-how-to-access-cave-2"));
                    editor.Data.Add("MasteryExtended_GMCM_HowToAccessCave3", ModEntry.ModHelper.Translation.Get("gmcm-how-to-access-cave-3"));
                    editor.Data.Add("MasteryExtended_GMCM_HowToAccessCave?", ModEntry.ModHelper.Translation.Get("gmcm-how-to-access-cave-?"));
                    editor.Data.Add("MasteryExtended_GMCM_CustomSkillsForCaveName", ModEntry.ModHelper.Translation.Get("gmcm-custom-skills-for-cave-name"));
                    editor.Data.Add("MasteryExtended_GMCM_CustomSkillsForCaveTooltip", ModEntry.ModHelper.Translation.Get("gmcm-custom-skills-for-cave-tooltip"));
                    editor.Data.Add("MasteryExtended_GMCM_SkillsRequiredForCaveName", ModEntry.ModHelper.Translation.Get("gmcm-skills-required-for-cave-name"));
                    editor.Data.Add("MasteryExtended_GMCM_SkillsRequiredForCaveTooltip", ModEntry.ModHelper.Translation.Get("gmcm-skills-required-for-cave-tooltip"));
                    editor.Data.Add("MasteryExtended_GMCM_MasteryRequiredForCaveName", ModEntry.ModHelper.Translation.Get("gmcm-mastery-required-for-cave-name"));
                    editor.Data.Add("MasteryExtended_GMCM_MasteryRequiredForCaveTooltip", ModEntry.ModHelper.Translation.Get("gmcm-mastery-required-for-cave-tooltip"));
                    editor.Data.Add("MasteryExtended_GMCM_CavePillarsAndPedestalSettingsTitle", ModEntry.ModHelper.Translation.Get("gmcm-cave-pillars-and-pedestal-settings-title"));
                    editor.Data.Add("MasteryExtended_GMCM_UnlockOrderCaveName", ModEntry.ModHelper.Translation.Get("gmcm-unlock-order-cave-name"));
                    editor.Data.Add("MasteryExtended_GMCM_UnlockOrderCaveTooltip", ModEntry.ModHelper.Translation.Get("gmcm-unlock-order-cave-tooltip"));
                    editor.Data.Add("MasteryExtended_GMCM_UnlockOrderCave0", ModEntry.ModHelper.Translation.Get("gmcm-unlock-order-cave-0"));
                    editor.Data.Add("MasteryExtended_GMCM_UnlockOrderCave1", ModEntry.ModHelper.Translation.Get("gmcm-unlock-order-cave-1"));
                    editor.Data.Add("MasteryExtended_GMCM_UnlockOrderCave2", ModEntry.ModHelper.Translation.Get("gmcm-unlock-order-cave-2"));
                    editor.Data.Add("MasteryExtended_GMCM_UnlockOrderCave?", ModEntry.ModHelper.Translation.Get("gmcm-unlock-order-cave-?"));
                    editor.Data.Add("MasteryExtended_GMCM_ProfessionsRequiredForPillarsName", ModEntry.ModHelper.Translation.Get("gmcm-professions-required-for-pillars-name"));
                    editor.Data.Add("MasteryExtended_GMCM_ProfessionsRequiredForPillarsTooltip", ModEntry.ModHelper.Translation.Get("gmcm-professions-required-for-pillars-tooltip"));
                    editor.Data.Add("MasteryExtended_GMCM_PillarsRequiredForProfessionsName", ModEntry.ModHelper.Translation.Get("gmcm-pillars-required-for-professions-name"));
                    editor.Data.Add("MasteryExtended_GMCM_PillarsRequiredForProfessionsTooltip", ModEntry.ModHelper.Translation.Get("gmcm-pillars-required-for-professions-tooltip"));
                    editor.Data.Add("MasteryExtended_GMCM_ConfirmProfessionAdquisitionName", ModEntry.ModHelper.Translation.Get("gmcm-confirm-profession-adquisition-name"));
                    editor.Data.Add("MasteryExtended_GMCM_ConfirmProfessionAdquisitionTooltip", ModEntry.ModHelper.Translation.Get("gmcm-confirm-profession-adquisition-tooltip"));

                    editor.Data.Add("MasteryExtended_GMCM_WoLCompatSettingsTitle", ModEntry.ModHelper.Translation.Get("gmcm-wol-compat-settings-title"));
                    editor.Data.Add("MasteryExtended_GMCM_PercentMasteryExperienceSharedName", ModEntry.ModHelper.Translation.Get("gmcm-percent-mastery-experience-shared-name"));
                    editor.Data.Add("MasteryExtended_GMCM_PercentMasteryExperienceSharedTooltip", ModEntry.ModHelper.Translation.Get("gmcm-percent-mastery-experience-shared-tooltip"));

                    editor.Data.Add("MasteryExtended_GMCM_VPPCompatSettingsTitle", ModEntry.ModHelper.Translation.Get("gmcm-vpp-compat-settings-title"));
                    editor.Data.Add("MasteryExtended_GMCM_VPPLvl10ForLvl15Name", ModEntry.ModHelper.Translation.Get("gmcm-vpp-lvl10-for-lvl15-name"));
                    editor.Data.Add("MasteryExtended_GMCM_VPPLvl10ForLvl15Tooltip", ModEntry.ModHelper.Translation.Get("gmcm-vpp-lvl10-for-lvl15-tooltip"));
                    editor.Data.Add("MasteryExtended_GMCM_VPPLvl15ForLvl20Name", ModEntry.ModHelper.Translation.Get("gmcm-vpp-lvl15-for-lvl20-name"));
                    editor.Data.Add("MasteryExtended_GMCM_VPPLvl15ForLvl20Tooltip", ModEntry.ModHelper.Translation.Get("gmcm-vpp-lvl15-for-lvl20-tooltip"));

                    editor.Data.Add("MasteryExtended_BookSkillFarmingMasteryBookName", ModEntry.ModHelper.Translation.Get("book-skill-farming-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookSkillFarmingMasteryBookDescription", ModEntry.ModHelper.Translation.Get("book-skill-farming-mastery-description"));
                    editor.Data.Add("MasteryExtended_BookSkillFishingMasteryBookName", ModEntry.ModHelper.Translation.Get("book-skill-fishing-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookSkillFishingMasteryBookDescription", ModEntry.ModHelper.Translation.Get("book-skill-fishing-mastery-description"));
                    editor.Data.Add("MasteryExtended_BookSkillForagingMasteryBookName", ModEntry.ModHelper.Translation.Get("book-skill-foraging-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookSkillForagingMasteryBookDescription", ModEntry.ModHelper.Translation.Get("book-skill-foraging-mastery-description"));
                    editor.Data.Add("MasteryExtended_BookSkillMiningMasteryBookName", ModEntry.ModHelper.Translation.Get("book-skill-mining-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookSkillMiningMasteryBookDescription", ModEntry.ModHelper.Translation.Get("book-skill-mining-mastery-description"));
                    editor.Data.Add("MasteryExtended_BookSkillCombatMasteryBookName", ModEntry.ModHelper.Translation.Get("book-skill-combat-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookSkillCombatMasteryBookDescription", ModEntry.ModHelper.Translation.Get("book-skill-combat-mastery-description"));

                    editor.Data.Add("MasteryExtended_BookCoopmasterMasteryBookName", ModEntry.ModHelper.Translation.Get("book-coopmaster-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookShepherdMasteryBookName", ModEntry.ModHelper.Translation.Get("book-shepherd-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookArtisanMasteryBookName", ModEntry.ModHelper.Translation.Get("book-artisan-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookAgriculturistMasteryBookName", ModEntry.ModHelper.Translation.Get("book-agriculturist-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookProfessionFarmingMasteryBookDescription", ModEntry.ModHelper.Translation.Get("book-profession-farming-mastery-description"));

                    editor.Data.Add("MasteryExtended_BookAnglerMasteryBookName", ModEntry.ModHelper.Translation.Get("book-angler-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookPirateMasteryBookName", ModEntry.ModHelper.Translation.Get("book-pirate-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookMarinerMasteryBookName", ModEntry.ModHelper.Translation.Get("book-mariner-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookLuremasterMasteryBookName", ModEntry.ModHelper.Translation.Get("book-luremaster-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookProfessionFishingMasteryBookDescription", ModEntry.ModHelper.Translation.Get("book-profession-fishing-mastery-description"));

                    editor.Data.Add("MasteryExtended_BookLumberjackMasteryBookName", ModEntry.ModHelper.Translation.Get("book-lumberjack-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookTapperMasteryBookName", ModEntry.ModHelper.Translation.Get("book-tapper-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookBotanistMasteryBookName", ModEntry.ModHelper.Translation.Get("book-botanist-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookTrackerMasteryBookName", ModEntry.ModHelper.Translation.Get("book-tracker-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookProfessionForagingMasteryBookDescription", ModEntry.ModHelper.Translation.Get("book-profession-foraging-mastery-description"));

                    editor.Data.Add("MasteryExtended_BookBlacksmithMasteryBookName", ModEntry.ModHelper.Translation.Get("book-blacksmith-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookProspectorMasteryBookName", ModEntry.ModHelper.Translation.Get("book-prospector-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookExcavatorMasteryBookName", ModEntry.ModHelper.Translation.Get("book-excavator-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookGemologistMasteryBookName", ModEntry.ModHelper.Translation.Get("book-gemologist-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookProfessionMiningMasteryBookDescription", ModEntry.ModHelper.Translation.Get("book-profession-mining-mastery-description"));

                    editor.Data.Add("MasteryExtended_BookBruteMasteryBookName", ModEntry.ModHelper.Translation.Get("book-brute-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookDefenderMasteryBookName", ModEntry.ModHelper.Translation.Get("book-defender-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookAcrobatMasteryBookName", ModEntry.ModHelper.Translation.Get("book-acrobat-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookDesperadoMasteryBookName", ModEntry.ModHelper.Translation.Get("book-desperado-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookProfessionCombatMasteryBookDescription", ModEntry.ModHelper.Translation.Get("book-profession-combat-mastery-description"));

                    editor.Data.Add("MasteryExtended_BookCompleteMasteryBookName", ModEntry.ModHelper.Translation.Get("book-complete-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookCompleteMasteryBookDescription", ModEntry.ModHelper.Translation.Get("book-complete-mastery-description"));

                    editor.Data.Add("MasteryExtended_GMCM_BookMasteryName", ModEntry.ModHelper.Translation.Get("gmcm-book-mastery-name"));
                    editor.Data.Add("MasteryExtended_GMCM_BookMasteryTooltip", ModEntry.ModHelper.Translation.Get("gmcm-book-mastery-tooltip"));
                    editor.Data.Add("MasteryExtended_GMCM_BookMasteryName0", ModEntry.ModHelper.Translation.Get("gmcm-book-mastery-0"));
                    editor.Data.Add("MasteryExtended_GMCM_BookMasteryName1", ModEntry.ModHelper.Translation.Get("gmcm-book-mastery-1"));
                    editor.Data.Add("MasteryExtended_GMCM_BookMasteryName2", ModEntry.ModHelper.Translation.Get("gmcm-book-mastery-2"));
                    editor.Data.Add("MasteryExtended_GMCM_BookMasteryName?", ModEntry.ModHelper.Translation.Get("gmcm-book-mastery-?"));
                });
            }
        }

        private static void BookPowersAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            BookPowerInfo[] books = [];

            if(e.NameWithoutLocale.IsEquivalentTo($"Tilesheets/{ModEntry.ModManifest.UniqueID}/MasteryBooks"))
            {
                e.LoadFromModFile<Texture2D>("assets/Books.png", AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                if (ModEntry.Config.BooksQuantity.Equals("0"))
                {
                    books = BookPowerListComplete();
                }
                else if (ModEntry.Config.BooksQuantity.Equals("1"))
                {
                    books = BookPowerListShort();
                }

                e.Edit(rawInfo =>
                {
                    var data = rawInfo.AsDictionary<string, ObjectData>().Data;

                    foreach (var book in books)
                    {
                        ObjectData toAdd = new()
                        {
                            Name = book.Id,
                            DisplayName = book.DisplayName,
                            Description = book.Description,
                            Type = "asdf", // Really? asdf?
                            Category = StardewValley.Object.booksCategory,
                            Price = 25000,
                            Texture = $"Tilesheets/{ModEntry.ModManifest.UniqueID}/MasteryBooks",
                            SpriteIndex = book.SpriteIndex,
                            ContextTags = book.ContextTags
                        };

                        data.TryAdd(book.Id, toAdd);
                    }
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
            {
                if (ModEntry.Config.BooksQuantity.Equals("0"))
                {
                    books = BookPowerListComplete();
                }
                else if (ModEntry.Config.BooksQuantity.Equals("1"))
                {
                    books = BookPowerListShort();
                }

                e.Edit(rawInfo =>
                {
                    IDictionary<string, ShopData> data = rawInfo.AsDictionary<string, ShopData>().Data;

                    foreach (var book in books)
                    {
                        ShopItemData toAdd = new()
                        {
                            Id = book.Id,
                            ItemId = book.Id,
                            Price = 25000,
                            AvailableStock = 1,
                            Condition = book.Condition
                        };
                        data["BookSeller"].Items.Add(toAdd);
                    }
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Powers"))
            {
                if (ModEntry.Config.BooksQuantity.Equals("0"))
                {
                    books = BookPowerListComplete();
                }
                else if (ModEntry.Config.BooksQuantity.Equals("1"))
                {
                    books = BookPowerListShort();
                }

                e.Edit(rawInfo =>
                {
                    var data = rawInfo.AsDictionary<string, PowersData>().Data;

                    foreach (var book in books)
                    {
                        PowersData toAdd = new()
                        {
                            DisplayName = book.DisplayName,
                            Description = book.Description,
                            TexturePath = $"Tilesheets/{ModEntry.ModManifest.UniqueID}/MasteryBooks",
                            TexturePosition = new Point(book.SpriteIndex%6 * 16, book.SpriteIndex/6 * 16),
                            UnlockedCondition = $"PLAYER_STAT Current {book.Id} 1"

                        };

                        data.TryAdd(book.Id, toAdd);
                    }
                });
            }
        }

        private static BookPowerInfo[] BookPowerListShort()
        {
            return [
                (
                    $"{ModEntry.ModManifest.UniqueID}_BookFarmingMastery",
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookSkillFarmingMasteryBookName"),
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookSkillFarmingMasteryBookDescription"),
                    24,
                    "PLAYER_HAS_PROFESSION Current 2",
                    ["book_item", "color_gold"]
                ),
                (
                    $"{ModEntry.ModManifest.UniqueID}_BookFishingMastery",
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookSkillFishingMasteryBookName"),
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookSkillFishingMasteryBookDescription"),
                    25,
                    "PLAYER_HAS_PROFESSION Current 8",
                    ["book_item", "color_blue"]
                ),
                (
                    $"{ModEntry.ModManifest.UniqueID}_BookForagingMastery",
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookSkillForagingMasteryBookName"),
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookSkillForagingMasteryBookDescription"),
                    26,
                    "PLAYER_HAS_PROFESSION Current 14",
                    ["book_item", "color_green"]
                ),
                (
                    $"{ModEntry.ModManifest.UniqueID}_BookMiningMastery",
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookSkillMiningMasteryBookName"),
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookSkillMiningMasteryBookDescription"),
                    27,
                    "PLAYER_HAS_PROFESSION Current 20",
                    ["book_item", "color_brown"]
                ),
                (
                    $"{ModEntry.ModManifest.UniqueID}_BookCombatMastery",
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookSkillCombatMasteryBookName"),
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookSkillCombatMasteryBookDescription"),
                    28,
                    "PLAYER_HAS_PROFESSION Current 26",
                    ["book_item", "color_red"]
                ),
                (
                    $"{ModEntry.ModManifest.UniqueID}_BookCompleteMastery",
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookCompleteMasteryBookName"),
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookCompleteMasteryBookDescription"),
                    5,
                    "PLAYER_VISITED_LOCATION Current MasteryCave",
                    ["book_item", "color_iridium"]
                ),
            ];

        }
        private static BookPowerInfo[] BookPowerListComplete()
        {
            return [
                (
                    $"{ModEntry.ModManifest.UniqueID}_BookCoopmasterMastery",
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookCoopmasterMasteryBookName"),
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookProfessionFarmingMasteryBookDescription"),
                    0,
                    "PLAYER_HAS_PROFESSION Current 2",
                    ["book_item", "color_gold"]
                ),
                (
                    $"{ModEntry.ModManifest.UniqueID}_BookShepherdMastery",
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookShepherdMasteryBookName"),
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookProfessionFarmingMasteryBookDescription"),
                    6,
                    "PLAYER_HAS_PROFESSION Current 3",
                    ["book_item", "color_gold"]
                ),
                (
                    $"{ModEntry.ModManifest.UniqueID}_BookArtisanMastery",
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookArtisanMasteryBookName"),
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookProfessionFarmingMasteryBookDescription"),
                    12,
                    "PLAYER_HAS_PROFESSION Current 4",
                    ["book_item", "color_gold"]
                ),
                (
                    $"{ModEntry.ModManifest.UniqueID}_BookAgriculturistMastery",
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookAgriculturistMasteryBookName"),
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookProfessionFarmingMasteryBookDescription"),
                    18,
                    "PLAYER_HAS_PROFESSION Current 5",
                    ["book_item", "color_gold"]
                ),
                (
                    $"{ModEntry.ModManifest.UniqueID}_BookAnglerMastery",
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookAnglerMasteryBookName"),
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookProfessionFishingMasteryBookDescription"),
                    1,
                    "PLAYER_HAS_PROFESSION Current 8",
                    ["book_item", "color_blue"]
                ),
                (
                    $"{ModEntry.ModManifest.UniqueID}_BookPirateMastery",
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookPirateMasteryBookName"),
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookProfessionFishingMasteryBookDescription"),
                    7,
                    "PLAYER_HAS_PROFESSION Current 9",
                    ["book_item", "color_blue"]
                ),
                (
                    $"{ModEntry.ModManifest.UniqueID}_BookMarinerMastery",
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookMarinerMasteryBookName"),
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookProfessionFishingMasteryBookDescription"),
                    13,
                    "PLAYER_HAS_PROFESSION Current 10",
                    ["book_item", "color_blue"]
                ),
                (
                    $"{ModEntry.ModManifest.UniqueID}_BookLuremasterMastery",
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookLuremasterMasteryBookName"),
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookProfessionFishingMasteryBookDescription"),
                    19,
                    "PLAYER_HAS_PROFESSION Current 11",
                    ["book_item", "color_blue"]
                ),
                (
                    $"{ModEntry.ModManifest.UniqueID}_BookLumberjackMastery",
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookLumberjackMasteryBookName"),
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookProfessionForagingMasteryBookDescription"),
                    2,
                    "PLAYER_HAS_PROFESSION Current 14",
                    ["book_item", "color_green"]
                ),
                (
                    $"{ModEntry.ModManifest.UniqueID}_BookTapperMastery",
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookTapperMasteryBookName"),
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookProfessionForagingMasteryBookDescription"),
                    8,
                    "PLAYER_HAS_PROFESSION Current 15",
                    ["book_item", "color_green"]
                ),
                (
                    $"{ModEntry.ModManifest.UniqueID}_BookBotanistMastery",
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookBotanistMasteryBookName"),
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookProfessionForagingMasteryBookDescription"),
                    14,
                    "PLAYER_HAS_PROFESSION Current 16",
                    ["book_item", "color_green"]
                ),
                (
                    $"{ModEntry.ModManifest.UniqueID}_BookTrackerMastery",
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookTrackerMasteryBookName"),
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookProfessionForagingMasteryBookDescription"),
                    20,
                    "PLAYER_HAS_PROFESSION Current 17",
                    ["book_item", "color_green"]
                ),
                (
                    $"{ModEntry.ModManifest.UniqueID}_BookBlacksmithMastery",
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookBlacksmithMasteryBookName"),
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookProfessionMiningMasteryBookDescription"),
                    3,
                    "PLAYER_HAS_PROFESSION Current 20",
                    ["book_item", "color_brown"]
                ),
                (
                    $"{ModEntry.ModManifest.UniqueID}_BookProspectorMastery",
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookProspectorMasteryBookName"),
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookProfessionMiningMasteryBookDescription"),
                    9,
                    "PLAYER_HAS_PROFESSION Current 21",
                    ["book_item", "color_brown"]
                ),
                (
                    $"{ModEntry.ModManifest.UniqueID}_BookExcavatorMastery",
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookExcavatorMasteryBookName"),
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookProfessionMiningMasteryBookDescription"),
                    15,
                    "PLAYER_HAS_PROFESSION Current 22",
                    ["book_item", "color_brown"]
                ),
                (
                    $"{ModEntry.ModManifest.UniqueID}_BookGemologistMastery",
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookGemologistMasteryBookName"),
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookProfessionMiningMasteryBookDescription"),
                    21,
                    "PLAYER_HAS_PROFESSION Current 23",
                    ["book_item", "color_brown"]
                ),
                (
                    $"{ModEntry.ModManifest.UniqueID}_BookBruteMastery",
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookBruteMasteryBookName"),
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookProfessionCombatMasteryBookDescription"),
                    4,
                    "PLAYER_HAS_PROFESSION Current 26",
                    ["book_item", "color_red"]
                ),
                (
                    $"{ModEntry.ModManifest.UniqueID}_BookDefenderMastery",
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookDefenderMasteryBookName"),
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookProfessionCombatMasteryBookDescription"),
                    10,
                    "PLAYER_HAS_PROFESSION Current 27",
                    ["book_item", "color_red"]
                ),
                (
                    $"{ModEntry.ModManifest.UniqueID}_BookAcrobatMastery",
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookAcrobatMasteryBookName"),
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookProfessionCombatMasteryBookDescription"),
                    16,
                    "PLAYER_HAS_PROFESSION Current 28",
                    ["book_item", "color_red"]
                ),
                (
                    $"{ModEntry.ModManifest.UniqueID}_BookDesperadoMastery",
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookDesperadoMasteryBookName"),
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookProfessionCombatMasteryBookDescription"),
                    22,
                    "PLAYER_HAS_PROFESSION Current 29",
                    ["book_item", "color_red"]
                ),
                (
                    $"{ModEntry.ModManifest.UniqueID}_BookCompleteMastery",
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookCompleteMasteryBookName"),
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_BookCompleteMasteryBookDescription"),
                    5,
                    "PLAYER_VISITED_LOCATION Current MasteryCave",
                    ["book_item", "color_iridium"]
                ),
            ];
        }

    }

    internal record struct BookPowerInfo(string Id, string DisplayName, string Description, int SpriteIndex, string Condition, List<string> ContextTags)
    {
        public static implicit operator (string Id, string DisplayName, string Description, int SpriteIndex, string Condition, List<string> ContextTags)(BookPowerInfo value)
        {
            return (value.Id, value.DisplayName, value.Description, value.SpriteIndex, value.Condition, value.ContextTags);
        }

        public static implicit operator BookPowerInfo((string Id, string DisplayName, string Description, int SpriteIndex, string Condition, List<string> ContextTags) value)
        {
            return new BookPowerInfo(value.Id, value.DisplayName, value.Description, value.SpriteIndex, value.Condition, value.ContextTags);
        }
    }
}
