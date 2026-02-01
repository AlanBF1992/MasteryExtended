using HarmonyLib;
using MasteryExtended.Compatibility.SpaceCore;
using MasteryExtended.Compatibility.VPP;
using MasteryExtended.Compatibility.WoL;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace MasteryExtended
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /************
        * Accessors *
        *************/
        /// <summary>Monitoring and logging for the mod.</summary>
        public static IMonitor LogMonitor { get; internal set; } = null!;

        /// <summary>Simplified APIs for writing mods.</summary>
        public static IModHelper ModHelper { get; internal set; } = null!;

        /// <summary>Manifest of the mod.</summary>
        new public static IManifest ModManifest { get; internal set; } = null!;

        /// <summary>The mod configuration from the player.</summary>
        public static ModConfig Config { get; internal set; } = null!;

        /// <summary>The mod data for the player.</summary>
        public static ModData Data { get; internal set; } = null!;

        /// <summary>Max Mastery Levels, including 5 for the pillars.</summary>
        public static int MaxMasteryLevels { get; internal set; } = 25;

        /// <summary>For VPP Changes.</summary>
        public static Func<int> MasteryCaveChanges { get; internal set; } = () => 10;

        /// <summary>Amount of Skills available</summary>
        public static int SkillsAvailable { get; internal set; } = 5;


        /******************
        ** Public methods *
        *******************/
        public override void Entry(IModHelper helper)
        {
            LogMonitor = Monitor;
            ModHelper = Helper;
            ModManifest = base.ModManifest;
            Config = helper.ReadConfig<ModConfig>();

            Harmony harmony = new(ModManifest.UniqueID);

            // Vanilla Patches
            VanillaLoader.Loader(helper, harmony);
            LogMonitor.Log("Base Patches Loaded", LogLevel.Info);

            // SpaceCore Compat
            if (helper.ModRegistry.IsLoaded("spacechase0.SpaceCore"))
            {
                SCLoader.Loader(helper, harmony);
                LogMonitor.Log("SpaceCore Compat Patches Loaded", LogLevel.Info);
            }

            // WoL Compat
            if (helper.ModRegistry.IsLoaded("DaLion.Professions"))
            {
                WoLLoader.Loader(helper, harmony);
                LogMonitor.Log("Walk of Life Compat Patches Loaded", LogLevel.Info);
            }

            // VPP Compat
            if (helper.ModRegistry.IsLoaded("KediDili.VanillaPlusProfessions"))
            {
                VPPLoader.Loader(helper, harmony);
                LogMonitor.Log("Vanilla Plus Profession Compat Patches Loaded", LogLevel.Info);
            }

            // Console commands
            ModCommands.addConsoleCommands(helper);

            // Events
            helper.Events.Content.AssetRequested += OnAssetRequested;
            helper.Events.GameLoop.SaveLoaded += ReadData;
            helper.Events.GameLoop.Saving += OnSaving;
        }

        /******************
        * Private methods *
        *******************/

        private static void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo(PathUtilities.NormalizeAssetName("Strings/UI")))
            {
                e.Edit(asset =>
                {
                    var editor = asset.AsDictionary<string, string>();

                    editor.Data.Add("MasteryExtended_InvestButton", ModHelper.Translation.Get("invest-button"));
                    editor.Data.Add("MasteryExtended_BackButton", ModHelper.Translation.Get("back-button"));
                    editor.Data.Add("MasteryExtended_NextButton", ModHelper.Translation.Get("next-button"));
                    editor.Data.Add("MasteryExtended_MenuTitleSkills", ModHelper.Translation.Get("menu-title-skills"));
                    editor.Data.Add("MasteryExtended_MenuTitleProfession", ModHelper.Translation.Get("menu-title-profession"));
                    editor.Data.Add("MasteryExtended_HoverSkill", ModHelper.Translation.Get("hover-skill"));
                    editor.Data.Add("MasteryExtended_AddedProfession", ModHelper.Translation.Get("added-profession"));
                    editor.Data.Add("MasteryExtended_NeedMoreProfessions", ModHelper.Translation.Get("need-more-professions"));
                    editor.Data.Add("MasteryExtended_NeedMoreLevels", ModHelper.Translation.Get("need-more-levels"));
                    editor.Data.Add("MasteryExtended_CantSpend", ModHelper.Translation.Get("cant-spend"));
                    editor.Data.Add("MasteryExtended_LookOnly", ModHelper.Translation.Get("look-only"));
                    editor.Data.Add("MasteryExtended_CantAccessSkill", ModHelper.Translation.Get("cant-access-skill"));
                    editor.Data.Add("MasteryExtended_EveryProfessionUnlocked", ModHelper.Translation.Get("every-profession-unlocked"));
                    editor.Data.Add("MasteryExtended_TrascendMortalKnowledge", ModHelper.Translation.Get("transcend-mortal-knowledge"));
                    editor.Data.Add("MasteryExtended_TrascendMortalKnowledgeOnly", ModHelper.Translation.Get("transcend-mortal-knowledge-only"));
                    editor.Data.Add("MasteryExtended_TrascendMortalKnowledgeTogether", ModHelper.Translation.Get("transcend-mortal-knowledge-together"));
                    editor.Data.Add("MasteryExtended_AlreadyUnlocked", ModHelper.Translation.Get("already-unlocked"));
                    editor.Data.Add("MasteryExtended_RequirementsTitle", ModHelper.Translation.Get("requirements-title"));
                    editor.Data.Add("MasteryExtended_RequirementsProfession", ModHelper.Translation.Get("requirements-profession"));
                    editor.Data.Add("MasteryExtended_RequirementsLvl10", ModHelper.Translation.Get("requirements-lvl10"));
                    editor.Data.Add("MasteryExtended_RequirementsLvl15", ModHelper.Translation.Get("requirements-lvl15"));
                    editor.Data.Add("MasteryExtended_RequirementsLvl20", ModHelper.Translation.Get("requirements-lvl20"));
                    editor.Data.Add("MasteryExtended_WoLMasteryWarning", ModHelper.Translation.Get("wol-mastery-warning"));

                    editor.Data.Add("MasteryExtended_GMCM_BasicSettingsTitle", ModHelper.Translation.Get("gmcm-basic-settings-title"));
                    editor.Data.Add("MasteryExtended_GMCM_MasteryExperienceName", ModHelper.Translation.Get("gmcm-mastery-experience-name"));
                    editor.Data.Add("MasteryExtended_GMCM_MasteryExperienceTooltip", ModHelper.Translation.Get("gmcm-mastery-experience-tooltip"));
                    editor.Data.Add("MasteryExtended_GMCM_SkillMenuTitle", ModHelper.Translation.Get("gmcm-skill-menu-settings-title"));
                    editor.Data.Add("MasteryExtended_GMCM_SkillNameHoverName", ModHelper.Translation.Get("gmcm-skill-name-on-hover-name"));
                    editor.Data.Add("MasteryExtended_GMCM_SkillNameHoverTooltip", ModHelper.Translation.Get("gmcm-skill-name-on-hover-tooltip"));
                    editor.Data.Add("MasteryExtended_GMCM_ProfessionNameHoverName", ModHelper.Translation.Get("gmcm-profession-name-on-hover-name"));
                    editor.Data.Add("MasteryExtended_GMCM_ProfessionNameHoverTooltip", ModHelper.Translation.Get("gmcm-profession-name-on-hover-tooltip"));
                    editor.Data.Add("MasteryExtended_GMCM_CaveAccessSettingsTitle", ModHelper.Translation.Get("gmcm-cave-access-settings-title"));
                    editor.Data.Add("MasteryExtended_GMCM_HowToAccessCaveName", ModHelper.Translation.Get("gmcm-how-to-access-cave-name"));
                    editor.Data.Add("MasteryExtended_GMCM_HowToAccessCaveTooltip", ModHelper.Translation.Get("gmcm-how-to-access-cave-tooltip"));
                    editor.Data.Add("MasteryExtended_GMCM_HowToAccessCave0", ModHelper.Translation.Get("gmcm-how-to-access-cave-0"));
                    editor.Data.Add("MasteryExtended_GMCM_HowToAccessCave1", ModHelper.Translation.Get("gmcm-how-to-access-cave-1"));
                    editor.Data.Add("MasteryExtended_GMCM_HowToAccessCave2", ModHelper.Translation.Get("gmcm-how-to-access-cave-2"));
                    editor.Data.Add("MasteryExtended_GMCM_HowToAccessCave3", ModHelper.Translation.Get("gmcm-how-to-access-cave-3"));
                    editor.Data.Add("MasteryExtended_GMCM_HowToAccessCave?", ModHelper.Translation.Get("gmcm-how-to-access-cave-?"));
                    editor.Data.Add("MasteryExtended_GMCM_CustomSkillsForCaveName", ModHelper.Translation.Get("gmcm-custom-skills-for-cave-name"));
                    editor.Data.Add("MasteryExtended_GMCM_CustomSkillsForCaveTooltip", ModHelper.Translation.Get("gmcm-custom-skills-for-cave-tooltip"));
                    editor.Data.Add("MasteryExtended_GMCM_SkillsRequiredForCaveName", ModHelper.Translation.Get("gmcm-skills-required-for-cave-name"));
                    editor.Data.Add("MasteryExtended_GMCM_SkillsRequiredForCaveTooltip", ModHelper.Translation.Get("gmcm-skills-required-for-cave-tooltip"));
                    editor.Data.Add("MasteryExtended_GMCM_MasteryRequiredForCaveName", ModHelper.Translation.Get("gmcm-mastery-required-for-cave-name"));
                    editor.Data.Add("MasteryExtended_GMCM_MasteryRequiredForCaveTooltip", ModHelper.Translation.Get("gmcm-mastery-required-for-cave-tooltip"));
                    editor.Data.Add("MasteryExtended_GMCM_CavePillarsAndPedestalSettingsTitle", ModHelper.Translation.Get("gmcm-cave-pillars-and-pedestal-settings-title"));
                    editor.Data.Add("MasteryExtended_GMCM_UnlockOrderCaveName", ModHelper.Translation.Get("gmcm-unlock-order-cave-name"));
                    editor.Data.Add("MasteryExtended_GMCM_UnlockOrderCaveTooltip", ModHelper.Translation.Get("gmcm-unlock-order-cave-tooltip"));
                    editor.Data.Add("MasteryExtended_GMCM_UnlockOrderCave0", ModHelper.Translation.Get("gmcm-unlock-order-cave-0"));
                    editor.Data.Add("MasteryExtended_GMCM_UnlockOrderCave1", ModHelper.Translation.Get("gmcm-unlock-order-cave-1"));
                    editor.Data.Add("MasteryExtended_GMCM_UnlockOrderCave2", ModHelper.Translation.Get("gmcm-unlock-order-cave-2"));
                    editor.Data.Add("MasteryExtended_GMCM_UnlockOrderCave?", ModHelper.Translation.Get("gmcm-unlock-order-cave-?"));
                    editor.Data.Add("MasteryExtended_GMCM_ProfessionsRequiredForPillarsName", ModHelper.Translation.Get("gmcm-professions-required-for-pillars-name"));
                    editor.Data.Add("MasteryExtended_GMCM_ProfessionsRequiredForPillarsTooltip", ModHelper.Translation.Get("gmcm-professions-required-for-pillars-tooltip"));
                    editor.Data.Add("MasteryExtended_GMCM_PillarsRequiredForProfessionsName", ModHelper.Translation.Get("gmcm-pillars-required-for-professions-name"));
                    editor.Data.Add("MasteryExtended_GMCM_PillarsRequiredForProfessionsTooltip", ModHelper.Translation.Get("gmcm-pillars-required-for-professions-tooltip"));
                    editor.Data.Add("MasteryExtended_GMCM_ConfirmProfessionAdquisitionName", ModHelper.Translation.Get("gmcm-confirm-profession-adquisition-name"));
                    editor.Data.Add("MasteryExtended_GMCM_ConfirmProfessionAdquisitionTooltip", ModHelper.Translation.Get("gmcm-confirm-profession-adquisition-tooltip"));

                    editor.Data.Add("MasteryExtended_GMCM_WoLCompatSettingsTitle", ModHelper.Translation.Get("gmcm-wol-compat-settings-title"));
                    editor.Data.Add("MasteryExtended_GMCM_PercentMasteryExperienceSharedName", ModHelper.Translation.Get("gmcm-percent-mastery-experience-shared-name"));
                    editor.Data.Add("MasteryExtended_GMCM_PercentMasteryExperienceSharedTooltip", ModHelper.Translation.Get("gmcm-percent-mastery-experience-shared-tooltip"));

                    editor.Data.Add("MasteryExtended_GMCM_VPPCompatSettingsTitle", ModHelper.Translation.Get("gmcm-vpp-compat-settings-title"));
                    editor.Data.Add("MasteryExtended_GMCM_VPPLvl10ForLvl15Name", ModHelper.Translation.Get("gmcm-vpp-lvl10-for-lvl15-name"));
                    editor.Data.Add("MasteryExtended_GMCM_VPPLvl10ForLvl15Tooltip", ModHelper.Translation.Get("gmcm-vpp-lvl10-for-lvl15-tooltip"));
                    editor.Data.Add("MasteryExtended_GMCM_VPPLvl15ForLvl20Name", ModHelper.Translation.Get("gmcm-vpp-lvl15-for-lvl20-name"));
                    editor.Data.Add("MasteryExtended_GMCM_VPPLvl15ForLvl20Tooltip", ModHelper.Translation.Get("gmcm-vpp-lvl15-for-lvl20-tooltip"));
                });
            }
        }

        /// <summary>Reads and applies the data when the save is loaded.</summary>
        private void ReadData(object? sender, SaveLoadedEventArgs e)
        {
            if (Context.IsMainPlayer)
            {
                // From here it checks the Data
                ModData? a = ModHelper.Data.ReadSaveData<ModData>("AlanBF.MasteryExtended");
                Data = a ?? new ModData();

                if (a is not null) return;

                ModCommands.recountUsedMasteryLevels();
                ModCommands.resetMasteryExp();
            }
            else
            {
                Data = new ModData
                {
                    claimedRewards = (int)Game1.player.stats.Get("mastery_total_pillars")
                };
            }
        }

        /// <summary>Save the data when the game is saved.</summary>
        private void OnSaving(object? sender, SavingEventArgs e)
        {
            if (Context.IsMainPlayer)
            {
                Helper.Data.WriteSaveData("AlanBF.MasteryExtended", Data);
            }
        }
    }
}
