using HarmonyLib;
using MasteryExtended.Compatibility.BGM;
using MasteryExtended.Compatibility.GMCM;
using MasteryExtended.Compatibility.SPU;
using MasteryExtended.Patches;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Constants;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Powers;
using StardewValley.GameData.Shops;
using StardewValley.Internal;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewValley.Tools;
using xTile.Dimensions;
using Object = StardewValley.Object;

namespace MasteryExtended
{
    internal static class VanillaLoader
    {
        internal static void Loader(IModHelper helper, Harmony harmony)
        {
            ModEntry.MaxMasteryLevels += 25;
            ModEntry.MaxMasteryLevels += 6;

            VanillaPatches(harmony);

            helper.Events.Content.AssetRequested += UpdateTentKit;
            helper.Events.Content.AssetRequested += UIStringsAssetRequested;
            helper.Events.Content.AssetRequested += TilesheetsAssetRequested;
            helper.Events.Content.AssetRequested += PowersAssetRequested;
            helper.Events.Content.AssetRequested += BooksAssetRequested;

            helper.Events.GameLoop.GameLaunched += SPUCompat;
            helper.Events.GameLoop.UpdateTicked += GMCMConfigVanilla;
            helper.Events.GameLoop.SaveLoaded += CalculatePillarsUnlocked;

            helper.Events.Display.MenuChanged += ReloadPowers;
        }

        private static void ReloadPowers(object? _, MenuChangedEventArgs e)
        {
            if (e.NewMenu is GameMenu ||
                ModEntry.ModHelper.ModRegistry.GetApi<IBetterGameMenuApi>("leclair.bettergamemenu")?.IsMenu(e.NewMenu) == true)
            {
                ModEntry.ModHelper.GameContent.InvalidateCache("Data\\Powers");
            }
        }

        /// <summary>Base patches for the mod.</summary>
        /// <param name="harmony">Harmony instance used to patch the game.</param>
        private static void VanillaPatches(Harmony harmony)
        {
            #region Experience and Mastery Gain
            /************************************
             * Farmer Experience & Mastery Gain *
             ************************************/
            // Changes how the level is calculated in Vanilla, since it expects the Luck to always be 0
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), "get_Level"),
                transpiler: new HarmonyMethod(typeof(FarmerPatch), nameof(FarmerPatch.LevelTranspiler))
            );

            // Changes how title information is provided with or without the Luck skill
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.getTitle)),
                transpiler: new HarmonyMethod(typeof(FarmerPatch), nameof(FarmerPatch.getTitleTranspiler))
            );

            // Increases the maximum Mastery level
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.getCurrentMasteryLevel)),
                postfix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.getCurrentMasteryLevelPostfix))
            );

            // Returns the experience needed for a new Mastery level
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.getMasteryExpNeededForLevel)),
                prefix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.getMasteryExpNeededForLevelPrefix))
            );

            // Allows gaining Mastery after a profession reaches level 10
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.gainExperience)),
                transpiler: new HarmonyMethod(typeof(FarmerPatch), nameof(FarmerPatch.gainExperienceTranspiler))
            );
            #endregion

            #region Mastery Bar and Numbers Drawing
            /*************************
             * Mastery Bar & Numbers *
             *************************/
            // Changes the Mastery Bar width and modifies the numbers
            harmony.Patch(
                original: AccessTools.Method(typeof(SkillsPage), nameof(SkillsPage.draw), [typeof(SpriteBatch)]),
                transpiler: new HarmonyMethod(typeof(SkillsPagePatch), nameof(SkillsPagePatch.drawTranspiler))
            );

            // Changes the Mastery level limit
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.drawBar)),
                transpiler: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.drawBarTranspiler))
            );
            #endregion

            #region Mastery Cave Map Changes
            /****************
             * Mastery Cave *
             ****************/
            // Allows alternative access to the Mastery Cave and changes the door message
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
            /***********************************
             * Mastery Cave Pedestal & Pillars *
             ***********************************/
            // Modifies the pedestal menu. Makes it taller and adds a button
            harmony.Patch(
                original: AccessTools.Constructor(typeof(MasteryTrackerMenu), [typeof(int)]),
                postfix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.ctorPostfix))
            );

            // Draw the button
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.draw), [typeof(SpriteBatch)]),
                postfix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.drawPostfix))
            );

            // Adds functionality to the button
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.receiveLeftClick)),
                prefix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.receiveLeftClickPrefix))
            );

            // Allows the button to have "animation"
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.update)),
                prefix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.updatePrefix))
            );

            // Allows the button to show why it cannot be claimed
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.performHoverAction)),
                postfix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.performHoverActionPostfix))
            );

            // Blocks the workaround for missing level 10 profession
            harmony.Patch(
                original: AccessTools.Method(typeof(LevelUpMenu), nameof(LevelUpMenu.AddMissedProfessionChoices)),
                prefix: new HarmonyMethod(typeof(LevelUpMenuPatch), nameof(LevelUpMenuPatch.AddMissedProfessionChoicesPrefix))
            );
            #endregion

            #region Show Acquired Professions
            // Changes the skillBars format to prevent drawing
            harmony.Patch(
                original: AccessTools.Constructor(typeof(SkillsPage), [typeof(int), typeof(int), typeof(int), typeof(int)]),
                transpiler: new HarmonyMethod(typeof(SkillsPagePatch), nameof(SkillsPagePatch.ctorTranspiler))
            );

            // Makes sure professions are shown when hovering
            harmony.Patch(
                original: AccessTools.Method(typeof(SkillsPage), nameof(SkillsPage.performHoverAction)),
                transpiler: new HarmonyMethod(typeof(SkillsPagePatch), nameof(SkillsPagePatch.performHoverActionTranspiler))
            );
            #endregion

            #region Others
            /**************
             * Dog Statue *
             **************/
            // Change Statue of Uncertainty to handle hidden powers
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction), [typeof(string[]), typeof(Farmer), typeof(Location)]),
                prefix: new HarmonyMethod(typeof(GameLocationPatch), nameof(GameLocationPatch.performActionPrefix))
            );

            // Profession Forget After Recount Used Mastery Levels
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
                postfix: new HarmonyMethod(typeof(GameLocationPatch), nameof(GameLocationPatch.answerDialogueActionPostFix))
            );

            /**********
             * Reaper * 
             **********/
            // Change Area of Effect of the Scythe
            harmony.Patch(
                original: AccessTools.Method(typeof(MeleeWeapon), nameof(MeleeWeapon.getAreaOfEffect)),
                postfix: new HarmonyMethod(typeof(MeleeWeaponPatch), nameof(MeleeWeaponPatch.getAreaOfEffectPostfix))
            );

            // Change the way the area is calculated so it works correctly
            if (!ModEntry.ModHelper.ModRegistry.IsLoaded("DaLion.Enchantments"))
            {
                harmony.Patch(
                    original: AccessTools.Method(typeof(MeleeWeapon), nameof(MeleeWeapon.DoDamage)),
                    transpiler: new HarmonyMethod(typeof(MeleeWeaponPatch), nameof(MeleeWeaponPatch.DoDamageTranspiler))
                );
            }

            /**************
             * Baitbinder *
             **************/
            // Add extra tries for normal fishes
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.GetFishFromLocationData), [typeof(string), typeof(Vector2), typeof(int), typeof(Farmer), typeof(bool), typeof(bool), typeof(GameLocation), typeof(ItemQueryContext)]),
                transpiler: new HarmonyMethod(typeof(GameLocationPatch), nameof(GameLocationPatch.GetFishFromLocationDataTranspiler))
            );

            // Add extra percentage for Cave fishes
            harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), nameof(MineShaft.getFish)),
                transpiler: new HarmonyMethod(typeof(MineShaftPatch), nameof(MineShaftPatch.getFishTranspiler))
            );

            // Add extra percentage for Crab Pots
            harmony.Patch(
                original: AccessTools.Method(typeof(CrabPot), nameof(CrabPot.DayUpdate)),
                transpiler: new HarmonyMethod(typeof(CrabPotPatch), nameof(CrabPotPatch.DayUpdateTranspiler))
            );

            // Allow Specific Bait to work even with the Mariner profession
            harmony.Patch(
                original: AccessTools.Method(typeof(CrabPot), nameof(CrabPot.performObjectDropInAction)),
                prefix: new HarmonyMethod(typeof(CrabPotPatch), nameof(CrabPotPatch.performObjectDropInActionPrefix))
            );

            /*********
             * Mason * 
             *********/
            // Extra stone, clay, marble or granite on every rock.
            harmony.Patch(
                original: AccessTools.Method(typeof(MineShaft), nameof(MineShaft.checkStoneForItems)),
                transpiler: new HarmonyMethod(typeof(MineShaftPatch), nameof(MineShaftPatch.checkStoneForItemsTranspiler))
            );

            /**************
             * Woodlander * 
             **************/
            // Add ModData to the tree when fertilizing it
            harmony.Patch(
                original: AccessTools.Method(typeof(Object), nameof(Object.placementAction)),
                transpiler: new HarmonyMethod(typeof(ObjectPatch), nameof(ObjectPatch.placementActionTranspiler))
            );

            // Grow to stage 5 if fertilized
            harmony.Patch(
                original: AccessTools.Method(typeof(Tree), nameof(Tree.dayUpdate)),
                transpiler: new HarmonyMethod(typeof(TreePatch), nameof(TreePatch.dayUpdateTranspiler))
            );

            // Allows tree to grow closer if fertilized
            harmony.Patch(
                original: AccessTools.Method(typeof(Tree), nameof(Tree.IsGrowthBlockedByNearbyTree)),
                prefix: new HarmonyMethod(typeof(TreePatch), nameof(TreePatch.IsGrowthBlockedByNearbyTreePrefix))
            );

            // Fertilized trees drop extra wood
            harmony.Patch(
                original: AccessTools.Method(typeof(Tree), nameof(Tree.tickUpdate)),
                transpiler: new HarmonyMethod(typeof(TreePatch), nameof(TreePatch.tickUpdateTranspiler))
            );

            // Fertilized trees drop extra wood from the stump
            harmony.Patch(
                original: AccessTools.Method(typeof(Tree), "performTreeFall"),
                transpiler: new HarmonyMethod(typeof(TreePatch), nameof(TreePatch.performTreeFallTranspiler))
            );

            /*************
             * Runesmith * 
             *************/
            // Any weapon can get innate enchantments
            harmony.Patch(
                original: AccessTools.Method(typeof(Tool), nameof(Tool.CanForge)),
                transpiler: new HarmonyMethod(typeof(ToolPatch), nameof(ToolPatch.CanForgeTranspiler))
            );

            // Innate enchantments are independent
            harmony.Patch(
                original: AccessTools.Method(typeof(MeleeWeapon), nameof(MeleeWeapon.attemptAddRandomInnateEnchantment)),
                prefix: new HarmonyMethod(typeof(MeleeWeaponPatch), nameof(MeleeWeaponPatch.attemptAddRandomInnateEnchantmentPrefix))
            );

            // Halves shards forging cost
            harmony.Patch(
                original: AccessTools.Method(typeof(ForgeMenu), nameof(ForgeMenu.GetForgeCost)),
                postfix: new HarmonyMethod(typeof(ForgeMenuPatch), nameof(ForgeMenuPatch.GetForgeCostPostfix))
            );

            /**************
             * Attractive * 
             **************/
            // +128 Magnetism
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.GetAppliedMagneticRadius)),
                postfix: new HarmonyMethod(typeof(FarmerPatch), nameof(FarmerPatch.GetAppliedMagneticRadiusPostfix))
            );
            #endregion
        }

        private static void SPUCompat(object? sender, GameLaunchedEventArgs e)
        {
            var spuApi = ModEntry.ModHelper.ModRegistry.GetApi<ISpecialPowerApi>("Spiderbuttons.SpecialPowerUtilities");
            if (spuApi is not null)
            {
                _ = spuApi.RegisterPowerCategory(ModEntry.ModManifest.UniqueID, () => "Mastery Extended", $"Tilesheets/{ModEntry.ModManifest.UniqueID}/PowersCategoryIcon");
            }
        }

        /// <summary>GMCM Compat Vanilla</summary>
        private static void GMCMConfigVanilla(object? _1, UpdateTickedEventArgs e)
        {
            if (e.Ticks < 10) return;
            ModEntry.ModHelper.Events.GameLoop.UpdateTicked -= GMCMConfigVanilla;
            // Get Generic Mod Config Menu's API (if it's installed)
            var configMenu = ModEntry.ModHelper.ModRegistry.GetApi<IGMCMApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;
            // Register mod
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

            // Books for extra Mastery Exp gain
            configMenu.AddTextOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.BooksQuantity.ToString("D"),
                setValue: value =>
                {
                    if (Enum.TryParse(value, true, out BooksQuantityOption option))
                    {
                        ModEntry.Config.BooksQuantity = option;
                    }
                    else
                    {
                        ModEntry.Config.BooksQuantity = (BooksQuantityOption)int.Parse(value);
                    }
                    ModEntry.ModHelper.GameContent.InvalidateCache("Data\\Objects");
                    ModEntry.ModHelper.GameContent.InvalidateCache("Data\\Shops");
                    ModEntry.ModHelper.GameContent.InvalidateCache("Data\\Powers");
                },
                name: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_BookMasteryName"),
                tooltip: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_BookMasteryTooltip", Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_BookMasteryName0")),
                allowedValues: [.. Enum.GetValues<BooksQuantityOption>().Cast<int>().Select(x => x.ToString())],
                formatAllowedValue: (value) =>
                {
                    return value switch
                    {
                        "0" or "1" or "2" => Game1.content.LoadString($"Strings\\UI:MasteryExtended_GMCM_BookMasteryName{value}"),
                        _ => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_BookMasteryName?"),
                    };
                }
            );

            // Book Price
            configMenu.AddNumberOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.BookPrice,
                setValue: (value) => ModEntry.Config.BookPrice = value,
                name: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_BookMasteryPriceName"),
                tooltip: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_BookMasteryPriceTooltip"),
                min: 15000,
                max: 35000,
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
                getValue: () => ModEntry.Config.SkillsVsMasteryPoints.ToString("D"),
                setValue: value =>
                {
                    if (Enum.TryParse(value, true, out SkillsVsMasteryPointsOption option))
                    {
                        ModEntry.Config.SkillsVsMasteryPoints = option;
                    }
                    else
                    {
                        ModEntry.Config.SkillsVsMasteryPoints = (SkillsVsMasteryPointsOption)int.Parse(value);
                    }
                },
                name: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_HowToAccessCaveName"),
                tooltip: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_HowToAccessCaveTooltip", Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_HowToAccessCave0")),
                allowedValues: [.. Enum.GetValues<SkillsVsMasteryPointsOption>().Cast<int>().Select(x => x.ToString())],
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

            /******************************
             * Mastery Cave Inner Working *
             ******************************/
            configMenu.AddSectionTitle(
                mod: ModEntry.ModManifest,
                text: () => Game1.content.LoadString("Strings\\UI:MasteryExtended_GMCM_CavePillarsAndPedestalSettingsTitle")
            );

            // Which way the Pillars unlock
            configMenu.AddTextOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.PillarsVsProfessions.ToString("D"),
                setValue: value =>
                {
                    if (Enum.TryParse(value, true, out PillarsVsProfessionsOption option))
                    {
                        ModEntry.Config.PillarsVsProfessions = option;
                    }
                    else
                    {
                        ModEntry.Config.PillarsVsProfessions = (PillarsVsProfessionsOption)int.Parse(value);
                    }
                },
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
        private static void UpdateTentKit(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/CraftingRecipes"))
            {
                if (Game1.player is not Farmer who
                    || !TreePatch.isFarmerWoodlander(who))
                {
                    return;
                }

                e.Edit(rawInfo =>
                {
                    var data = rawInfo.AsDictionary<string, string>().Data;

                    data["Tent Kit"] = data["Tent Kit"].Replace("TentKit", "TentKit 2");
                });
            }
        }

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

                    editor.Data.Add("MasteryExtended_GMCM_BookMasteryName", ModEntry.ModHelper.Translation.Get("gmcm-book-mastery-name"));
                    editor.Data.Add("MasteryExtended_GMCM_BookMasteryTooltip", ModEntry.ModHelper.Translation.Get("gmcm-book-mastery-tooltip"));
                    editor.Data.Add("MasteryExtended_GMCM_BookMasteryName0", ModEntry.ModHelper.Translation.Get("gmcm-book-mastery-0"));
                    editor.Data.Add("MasteryExtended_GMCM_BookMasteryName1", ModEntry.ModHelper.Translation.Get("gmcm-book-mastery-1"));
                    editor.Data.Add("MasteryExtended_GMCM_BookMasteryName2", ModEntry.ModHelper.Translation.Get("gmcm-book-mastery-2"));
                    editor.Data.Add("MasteryExtended_GMCM_BookMasteryName?", ModEntry.ModHelper.Translation.Get("gmcm-book-mastery-?"));

                    editor.Data.Add("MasteryExtended_GMCM_BookMasteryPriceName", ModEntry.ModHelper.Translation.Get("gmcm-book-mastery-price-name"));
                    editor.Data.Add("MasteryExtended_GMCM_BookMasteryPriceTooltip", ModEntry.ModHelper.Translation.Get("gmcm-book-mastery-price-tooltip"));

                    editor.Data.Add("MasteryExtended_GMCM_VPPCompatSettingsTitle", ModEntry.ModHelper.Translation.Get("gmcm-vpp-compat-settings-title"));
                    editor.Data.Add("MasteryExtended_GMCM_VPPLvl10ForLvl15Name", ModEntry.ModHelper.Translation.Get("gmcm-vpp-lvl10-for-lvl15-name"));
                    editor.Data.Add("MasteryExtended_GMCM_VPPLvl10ForLvl15Tooltip", ModEntry.ModHelper.Translation.Get("gmcm-vpp-lvl10-for-lvl15-tooltip"));
                    editor.Data.Add("MasteryExtended_GMCM_VPPLvl15ForLvl20Name", ModEntry.ModHelper.Translation.Get("gmcm-vpp-lvl15-for-lvl20-name"));
                    editor.Data.Add("MasteryExtended_GMCM_VPPLvl15ForLvl20Tooltip", ModEntry.ModHelper.Translation.Get("gmcm-vpp-lvl15-for-lvl20-tooltip"));

                    editor.Data.Add("MasteryExtended_BookMastery_SkillFarming_BookName", ModEntry.ModHelper.Translation.Get("book-skill-farming-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookMastery_SkillFarming_BookDescription", ModEntry.ModHelper.Translation.Get("book-skill-farming-mastery-description"));
                    editor.Data.Add("MasteryExtended_BookMastery_SkillFishing_BookName", ModEntry.ModHelper.Translation.Get("book-skill-fishing-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookMastery_SkillFishing_BookDescription", ModEntry.ModHelper.Translation.Get("book-skill-fishing-mastery-description"));
                    editor.Data.Add("MasteryExtended_BookMastery_SkillForaging_BookName", ModEntry.ModHelper.Translation.Get("book-skill-foraging-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookMastery_SkillForaging_BookDescription", ModEntry.ModHelper.Translation.Get("book-skill-foraging-mastery-description"));
                    editor.Data.Add("MasteryExtended_BookMastery_SkillMining_BookName", ModEntry.ModHelper.Translation.Get("book-skill-mining-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookMastery_SkillMining_BookDescription", ModEntry.ModHelper.Translation.Get("book-skill-mining-mastery-description"));
                    editor.Data.Add("MasteryExtended_BookMastery_SkillCombat_BookName", ModEntry.ModHelper.Translation.Get("book-skill-combat-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookMastery_SkillCombat_BookDescription", ModEntry.ModHelper.Translation.Get("book-skill-combat-mastery-description"));

                    editor.Data.Add("MasteryExtended_BookMastery_ProfessionCoopmaster_BookName", ModEntry.ModHelper.Translation.Get("book-coopmaster-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookMastery_ProfessionShepherd_BookName", ModEntry.ModHelper.Translation.Get("book-shepherd-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookMastery_ProfessionArtisan_BookName", ModEntry.ModHelper.Translation.Get("book-artisan-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookMastery_ProfessionAgriculturist_BookName", ModEntry.ModHelper.Translation.Get("book-agriculturist-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookMastery_ProfessionFarming_BookDescription", ModEntry.ModHelper.Translation.Get("book-profession-farming-mastery-description"));

                    editor.Data.Add("MasteryExtended_BookMastery_ProfessionAngler_BookName", ModEntry.ModHelper.Translation.Get("book-angler-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookMastery_ProfessionPirate_BookName", ModEntry.ModHelper.Translation.Get("book-pirate-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookMastery_ProfessionMariner_BookName", ModEntry.ModHelper.Translation.Get("book-mariner-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookMastery_ProfessionLuremaster_BookName", ModEntry.ModHelper.Translation.Get("book-luremaster-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookMastery_ProfessionFishing_BookDescription", ModEntry.ModHelper.Translation.Get("book-profession-fishing-mastery-description"));

                    editor.Data.Add("MasteryExtended_BookMastery_ProfessionLumberjack_BookName", ModEntry.ModHelper.Translation.Get("book-lumberjack-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookMastery_ProfessionTapper_BookName", ModEntry.ModHelper.Translation.Get("book-tapper-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookMastery_ProfessionBotanist_BookName", ModEntry.ModHelper.Translation.Get("book-botanist-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookMastery_ProfessionTracker_BookName", ModEntry.ModHelper.Translation.Get("book-tracker-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookMastery_ProfessionForaging_BookDescription", ModEntry.ModHelper.Translation.Get("book-profession-foraging-mastery-description"));

                    editor.Data.Add("MasteryExtended_BookMastery_ProfessionBlacksmith_BookName", ModEntry.ModHelper.Translation.Get("book-blacksmith-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookMastery_ProfessionProspector_BookName", ModEntry.ModHelper.Translation.Get("book-prospector-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookMastery_ProfessionExcavator_BookName", ModEntry.ModHelper.Translation.Get("book-excavator-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookMastery_ProfessionGemologist_BookName", ModEntry.ModHelper.Translation.Get("book-gemologist-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookMastery_ProfessionMining_BookDescription", ModEntry.ModHelper.Translation.Get("book-profession-mining-mastery-description"));

                    editor.Data.Add("MasteryExtended_BookMastery_ProfessionBrute_BookName", ModEntry.ModHelper.Translation.Get("book-brute-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookMastery_ProfessionDefender_BookName", ModEntry.ModHelper.Translation.Get("book-defender-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookMastery_ProfessionAcrobat_BookName", ModEntry.ModHelper.Translation.Get("book-acrobat-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookMastery_ProfessionDesperado_BookName", ModEntry.ModHelper.Translation.Get("book-desperado-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookMastery_ProfessionCombat_BookDescription", ModEntry.ModHelper.Translation.Get("book-profession-combat-mastery-description"));

                    editor.Data.Add("MasteryExtended_BookMastery_Unlock_BookName", ModEntry.ModHelper.Translation.Get("book-unlock-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookMastery_Unlock_BookDescription", ModEntry.ModHelper.Translation.Get("book-unlock-mastery-description"));

                    editor.Data.Add("MasteryExtended_BookMastery_Complete_BookName", ModEntry.ModHelper.Translation.Get("book-complete-mastery-name"));
                    editor.Data.Add("MasteryExtended_BookMastery_Complete_BookDescription", ModEntry.ModHelper.Translation.Get("book-complete-mastery-description"));

                    editor.Data.Add("MasteryExtended_BookPower_FarmingMastery_BookDescription", ModEntry.ModHelper.Translation.Get("power-skill-farming-mastery-description"));
                    editor.Data.Add("MasteryExtended_BookPower_FishingMastery_BookDescription", ModEntry.ModHelper.Translation.Get("power-skill-fishing-mastery-description"));
                    editor.Data.Add("MasteryExtended_BookPower_ForagingMastery_BookDescription", ModEntry.ModHelper.Translation.Get("power-skill-foraging-mastery-description"));
                    editor.Data.Add("MasteryExtended_BookPower_MiningMastery_BookDescription", ModEntry.ModHelper.Translation.Get("power-skill-mining-mastery-description"));
                    editor.Data.Add("MasteryExtended_BookPower_CombatMastery_BookDescription", ModEntry.ModHelper.Translation.Get("power-skill-combat-mastery-description"));

                    editor.Data.Add("MasteryExtended_ReaperName", ModEntry.ModHelper.Translation.Get("power-dog-reaper-name"));
                    editor.Data.Add("MasteryExtended_ReaperDescription", ModEntry.ModHelper.Translation.Get("power-dog-reaper-description"));
                    editor.Data.Add("MasteryExtended_MasonName", ModEntry.ModHelper.Translation.Get("power-dog-mason-name"));
                    editor.Data.Add("MasteryExtended_MasonDescription", ModEntry.ModHelper.Translation.Get("power-dog-mason-description"));
                    editor.Data.Add("MasteryExtended_WoodlanderName", ModEntry.ModHelper.Translation.Get("power-dog-woodlander-name"));
                    editor.Data.Add("MasteryExtended_WoodlanderDescription", ModEntry.ModHelper.Translation.Get("power-dog-woodlander-description"));
                    editor.Data.Add("MasteryExtended_BaitbinderName", ModEntry.ModHelper.Translation.Get("power-dog-baitbinder-name"));
                    editor.Data.Add("MasteryExtended_BaitbinderDescription", ModEntry.ModHelper.Translation.Get("power-dog-baitbinder-description"));
                    editor.Data.Add("MasteryExtended_RunesmithName", ModEntry.ModHelper.Translation.Get("power-dog-runesmith-name"));
                    editor.Data.Add("MasteryExtended_RunesmithDescription", ModEntry.ModHelper.Translation.Get("power-dog-runesmith-description"));
                    editor.Data.Add("MasteryExtended_AttractiveName", ModEntry.ModHelper.Translation.Get("power-dog-attractive-name"));
                    editor.Data.Add("MasteryExtended_AttractiveDescription", ModEntry.ModHelper.Translation.Get("power-dog-attractive-description"));
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo(PathUtilities.NormalizeAssetName("Strings/Locations")))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsDictionary<string, string>();

                    editor.Data.Add("MasteryExtended_DogStatueMenuQuestionMainDialogue", ModEntry.ModHelper.Translation.Get("dog-statue-menu-question-main-dialogue"));
                    editor.Data.Add("MasteryExtended_DogStatueMenuQuestionMainOptionPower", ModEntry.ModHelper.Translation.Get("dog-statue-menu-question-main-option-power"));
                    editor.Data.Add("MasteryExtended_DogStatueMenuQuestionMainOptionReset", ModEntry.ModHelper.Translation.Get("dog-statue-menu-question-main-option-reset"));
                    editor.Data.Add("MasteryExtended_DogStatueMenuGotAllPowers", ModEntry.ModHelper.Translation.Get("dog-statue-menu-got-all-powers"));
                    editor.Data.Add("MasteryExtended_DogStatueMenuNeedMoreMasteryOrLevels", ModEntry.ModHelper.Translation.Get("dog-statue-menu-need-more-mastery-or-levels"));
                    editor.Data.Add("MasteryExtended_DogStatueMenuQuestionWhatPowerDialogue", ModEntry.ModHelper.Translation.Get("dog-statue-menu-question-what-power-dialogue"));
                    editor.Data.Add("MasteryExtended_DogStatueMenuQuestionUnlockDialogue", ModEntry.ModHelper.Translation.Get("dog-statue-menu-question-unlock-dialogue"));
                });
            }
        }

        // Tilesheets
        private static void TilesheetsAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo($"Tilesheets/{ModEntry.ModManifest.UniqueID}/DogPowers"))
            {
                e.LoadFromModFile<Texture2D>("assets/DogPowers.png", AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo($"Tilesheets/{ModEntry.ModManifest.UniqueID}/MasteryBooks"))
            {
                e.LoadFromModFile<Texture2D>("assets/Books.png", AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo($"Tilesheets/{ModEntry.ModManifest.UniqueID}/PowersCategoryIcon"))
            {
                e.LoadFromModFile<Texture2D>("assets/PowersCategory.png", AssetLoadPriority.Exclusive);
            }
        }

        // Powers
        private static void PowersAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (ModEntry.Config.BooksQuantity == BooksQuantityOption.None) return;

            if (e.NameWithoutLocale.IsEquivalentTo("Data/Powers"))
            {
                e.Edit(rawInfo =>
                {
                    var data = rawInfo.AsDictionary<string, PowersData>().Data;

                    foreach (PowerInfo powerInfo in PowerInfo.PowerList)
                    {
                        PowersData toAdd = new()
                        {
                            DisplayName = Game1.content.LoadString(powerInfo.DisplayNamePath),
                            Description = Game1.content.LoadString(powerInfo.PowerDescriptionPath, powerInfo.GetSubstitutions()),
                            TexturePath = powerInfo.TexturePath,
                            TexturePosition = new Point(powerInfo.SpriteIndex % 6 * 16, powerInfo.SpriteIndex / 6 * 16),
                            UnlockedCondition = powerInfo.PowerUnlockCondition
                        };

                        data.TryAdd(powerInfo.Id, toAdd);
                    }
                });
            }
        }

        // Books Object and Bookseller
        private static void BooksAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            IReadOnlyList<BookInfo> books = ModEntry.Config.BooksQuantity switch
            {
                BooksQuantityOption.Full => BookInfo.BookPowerListComplete,
                BooksQuantityOption.Lite => BookInfo.BookPowerListShort,
                _ => []
            };

            if (books.Count == 0) return;

            if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                string texture = $"Tilesheets/{ModEntry.ModManifest.UniqueID}/MasteryBooks";
                int price = ModEntry.Config.BookPrice;

                e.Edit(rawInfo =>
                {
                    var data = rawInfo.AsDictionary<string, ObjectData>().Data;

                    foreach (var book in books)
                    {
                        ObjectData toAdd = new()
                        {
                            Name = book.Id,
                            DisplayName = Game1.content.LoadString(book.DisplayNamePath),
                            Description = Game1.content.LoadString(book.BookDescriptionPath),
                            Type = "asdf", // The type for books is "asdf" according to the code
                            Category = Object.booksCategory,
                            Price = price,
                            Texture = texture,
                            SpriteIndex = book.SpriteIndex,
                            ContextTags = book.ContextTags.ToList()
                        };

                        data.TryAdd(book.Id, toAdd);
                    }
                });
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Shops"))
            {
                int price = ModEntry.Config.BookPrice;

                e.Edit(rawInfo =>
                {
                    IDictionary<string, ShopData> data = rawInfo.AsDictionary<string, ShopData>().Data;

                    foreach (var book in books)
                    {
                        ShopItemData toAdd = new()
                        {
                            Id = book.Id,
                            ItemId = book.Id,
                            Price = price,
                            AvailableStock = 1,
                            Condition = book.ShopCondition
                        };
                        data["Bookseller"].Items.Add(toAdd);
                    }
                });
            }
        }
    }
}
