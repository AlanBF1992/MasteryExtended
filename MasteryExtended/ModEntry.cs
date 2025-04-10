using HarmonyLib;
using MasteryExtended.Compatibility.SpaceCore;
using MasteryExtended.Compatibility.VPP;
using MasteryExtended.Compatibility.WoL;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;

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

        /// <summary>Simplified APIs for writing mods.</summary>
        new public static IManifest ModManifest { get; internal set; } = null!;

        /// <summary>The mod configuration from the player.</summary>
        public static ModConfig Config { get; internal set; } = null!;

        /// <summary>The mod data for the player.</summary>
        public static ModData Data { get; internal set; } = null!;

        /// <summary>Max Mastery Levels, including 5 for the pillars.</summary>
        public static int MaxMasteryLevels { get; internal set; } = 25;

        /// <summary>For VPP Changes.</summary>
        public static Func<bool> MasteryCaveChanges { get; internal set; } = () => false;

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
                LogMonitor.Log("Vanilla Plus Profession Compat Patches Loaded", LogLevel.Info);
                VPPLoader.Loader(helper, harmony);
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
                    editor.Data.Add("MasteryExtended_AlreadyUnlocked", ModEntry.ModHelper.Translation.Get("already-unlocked"));
                    editor.Data.Add("MasteryExtended_RequirementsTitle", ModEntry.ModHelper.Translation.Get("requirements-title"));
                    editor.Data.Add("MasteryExtended_RequirementsProfession", ModEntry.ModHelper.Translation.Get("requirements-profession"));
                    editor.Data.Add("MasteryExtended_RequirementsLvl10", ModEntry.ModHelper.Translation.Get("requirements-lvl10"));
                    editor.Data.Add("MasteryExtended_RequirementsLvl15", ModEntry.ModHelper.Translation.Get("requirements-lvl15"));
                    editor.Data.Add("MasteryExtended_RequirementsLvl20", ModEntry.ModHelper.Translation.Get("requirements-lvl20"));
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
                    claimedRewards = Utilities.countClaimedPillars()
                };
            }
        }

        /// <summary>Save the data when the game is saved.</summary>
        private void OnSaving(object? sender, SavingEventArgs e)
        {
            if (Context.IsMainPlayer)
            {
                base.Helper.Data.WriteSaveData("AlanBF.MasteryExtended", Data);
            }
        }
    }
}
