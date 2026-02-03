using HarmonyLib;
using MasteryExtended.Compatibility.SpaceCore;
using MasteryExtended.Compatibility.VPP;
using MasteryExtended.Compatibility.WoL;
using StardewModdingAPI;
using StardewModdingAPI.Events;
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
            helper.Events.GameLoop.SaveLoaded += ReadData;
            helper.Events.GameLoop.Saving += OnSaving;
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
