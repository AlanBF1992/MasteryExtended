using HarmonyLib;
using MasteryExtended.Patches;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using xTile.Dimensions;
using Microsoft.Xna.Framework.Graphics;
using MasteryExtended.Skills;
using MasteryExtended.Menu.Pages;

namespace MasteryExtended
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /************************
        ** Fields
        ************************/
        /// <summary>The mod configuration from the player.</summary>
        internal static ModConfig Config = null!;
        /// <summary>The mod data for the player.</summary>
        internal static ModData Data = null!;
        /// <summary>Log info.</summary>
        public static IMonitor LogMonitor { get; private set; } = null!;
        /// <summary>Max Mastery for the Mod.</summary>
        public static int MaxMasteryPoints { get; set; } = 25;
        // Helper?
        public static IModHelper ModHelper { get; private set; } = null!;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();

            ModHelper = helper;

            LogMonitor = Monitor; // Solo aca empieza a existir

            /// Insertar Parches necesarios
            var harmony = new Harmony(this.ModManifest.UniqueID);
            applyPatches(harmony);

            //Events
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.Saving += this.OnSaving;
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
        }

        /*********
        ** Private methods
        *********/
        /// <summary>Apply the patches for the mod.</summary>
        /// <param name="harmony">Harmony instance used to patch the game.</param>
        private void applyPatches(Harmony harmony)
        {
            /**********************
             * Farmer Mastery Gain
             **********************/
            // Permite ganar Mastery desde que se llega al máximo de la primera profesión
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.gainExperience)),
                prefix: new HarmonyMethod(typeof(FarmerPatch), nameof(FarmerPatch.gainExperiencePrefix))
            );
            // Aumenta el máximo nivel de Mastery
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.getCurrentMasteryLevel)),
                postfix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.getCurrentMasteryLevelPostfix))
            );

            /**************
             * Mastery Bar
             **************/
            // Modifica la barra de maestría, en el menú y en el pedestal
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.drawBar)),
                prefix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.drawBarPrefix))
            );

            // Modificaciones para que eliminar el hover
            harmony.Patch(
                original: AccessTools.Method(typeof(SkillsPage), nameof(SkillsPage.draw), new Type[] { typeof(SpriteBatch) }),
                prefix: new HarmonyMethod(typeof(SkillsPagePatch), nameof(SkillsPagePatch.drawPrefix))
            );

            // Modifica el nivel mostrado y devuelve el hover en la página de habilidades
            harmony.Patch(
                original: AccessTools.Method(typeof(SkillsPage), nameof(SkillsPage.draw), new Type[] { typeof(SpriteBatch) }),
                postfix: new HarmonyMethod(typeof(SkillsPagePatch), nameof(SkillsPagePatch.drawPostfix))
            );

            /********************
             * Mastery Cave Door
             ********************/
            // Al hacer click en la puerta, te permite acceder antes y te dice como
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction), new Type[] { typeof(string[]), typeof(Farmer), typeof(Location) }),
                prefix: new HarmonyMethod(typeof(GameLocationPatch), nameof(GameLocationPatch.performActionPrefix))
            );

            /************************
             * Mastery Cave Pedestal
             ************************/
            // Modifica el menú del pedestal. Lo hace más alto y crea un botón
            harmony.Patch(
                original: AccessTools.Constructor(typeof(MasteryTrackerMenu), new Type[] { typeof(int) }),
                postfix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.MasteryTrackerMenuPostfix))
            );

            // Dibuja el botón
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.draw), new Type[] { typeof(SpriteBatch) }),
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
        }

        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
        }

        /// <summary>Reads and applies the data when the save is loaded.</summary>
        [EventPriority(EventPriority.Low)]
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            // From here it checks the Data
            ModData? a = Helper.Data.ReadSaveData<ModData>("AlanBF.MasteryExtended");
            ModData? b = Helper.Data.ReadJsonFile<ModData>($"data/{Constants.SaveFolderName}.json");
            Data = a ?? new ModData();

            if (a is not null) return;

            if (b is not null)
            {
                Data.claimedRewards = b.claimedRewards;
                LogMonitor.Log("The data was loaded correctly. Delete the json file in the mod folder.",LogLevel.Warn);
                return;
            }

            List<string> checkRecipeList = new() { "Statue Of Blessings", "Heavy Furnace", "Challenge Bait", "Treasure Totem", "Anvil" };
            int spentLevelsInMasteryPillar = checkRecipeList.Count(x => Game1.player.craftingRecipes.ContainsKey(x));

            Data.claimedRewards += spentLevelsInMasteryPillar;

            int spentLevelsInProfessions = 0;

            foreach (Skill s in MasterySkillsPage.skills)
            {
                spentLevelsInProfessions += Math.Max(s.unlockedProfessions() - 2, 0);
            }

            int totalSpentLevels = spentLevelsInMasteryPillar + spentLevelsInProfessions;

            int currentMasteryExp = (int)Game1.stats.Get("MasteryExp");

            int expToSet = totalSpentLevels <= 5 ?
                Math.Max(MasteryTrackerMenu.getMasteryExpNeededForLevel(totalSpentLevels),
                            Math.Min(currentMasteryExp, MasteryTrackerMenu.getMasteryExpNeededForLevel(5))) :
                MasteryTrackerMenu.getMasteryExpNeededForLevel(5) + (totalSpentLevels - 5) * Config.MasteryExpPerLevel;

            Game1.stats.Set("MasteryExp", expToSet);
            Game1.stats.Set("masteryLevelsSpent", totalSpentLevels);
        }

        /// <summary>Save the data when the game is saved.</summary>
        private void OnSaving(object? sender, SavingEventArgs e)
        {
            Helper.Data.WriteSaveData("AlanBF.MasteryExtended", Data);
        }
    }
}
