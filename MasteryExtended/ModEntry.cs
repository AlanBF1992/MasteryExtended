using HarmonyLib;
using MasteryExtended.Menu.Pages;
using MasteryExtended.Patches;
using MasteryExtended.Skills;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using xTile.Dimensions;

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

            //Console command
            helper.ConsoleCommands.Add(
                "masteryExtended_RecountUsed",
                "Recounts the player's Mastery Level Used.",
                (_, __) => recountUsedMastery());

            helper.ConsoleCommands.Add(
                "masteryExtended_RestartExp",
                "Sets the Mastery Level to the minimum possible.",
                (_, __) => recountExpMastery());

            helper.ConsoleCommands.Add(
                "masteryExtended_RestartProffesions",
                "Restart Vanilla Proffesions when you sleep.",
                (_, __) => { clearAllProfessions(); recountUsedMastery(); });
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
            // Devuelve la experiencia necesaria
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.getMasteryExpNeededForLevel)),
                prefix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.getMasteryExpNeededForLevelPrefix))
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

        /// <summary>Reads and applies the data when the save is loaded.</summary>
        [EventPriority(EventPriority.Low)]
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            // From here it checks the Data
            ModData? a = Helper.Data.ReadSaveData<ModData>("AlanBF.MasteryExtended");
            ModData? b = Helper.Data.ReadJsonFile<ModData>($"data/{Constants.SaveFolderName}.json");
            Data = a ?? new ModData();

            if (b is not null)
            {
                Data.claimedRewards = b.claimedRewards;
                LogMonitor.Log("The data was loaded correctly. The json has been deleted.", LogLevel.Warn);
                Helper.Data.WriteSaveData("AlanBF.MasteryExtended", Data);
                string dataFolder = Path.Combine(Helper.DirectoryPath, "data");
                Directory.Delete(dataFolder, true);
                return;
            }

            if (a is not null) return;

            recountUsedMastery();
            recountExpMastery();
        }

        public static void recountUsedMastery()
        {
            if (Game1.player is null)
            {
                LogMonitor.Log("You need to load a save for the command to work");
                return;
            }

            List<string> checkRecipeList = new() { "Statue Of Blessings", "Heavy Furnace", "Challenge Bait", "Treasure Totem", "Anvil" };
            int spentLevelsInMasteryPillar = checkRecipeList.Count(x => Game1.player.craftingRecipes.ContainsKey(x));

            Data.claimedRewards = spentLevelsInMasteryPillar;

            int spentLevelsInProfessions = 0;

            foreach (Skill s in MasterySkillsPage.skills)
            {
                spentLevelsInProfessions += Math.Max(s.unlockedProfessions() - 2, 0);
            }

            int totalSpentLevels = spentLevelsInMasteryPillar + spentLevelsInProfessions;

            Game1.stats.Set("masteryLevelsSpent", totalSpentLevels);
        }

        private static void recountExpMastery()
        {
            if (Game1.player is null)
            {
                LogMonitor.Log("You need to load a save for the command to work");
                return;
            }
            int totalSpentLevels = (int)Game1.stats.Get("masteryLevelsSpent");
            int currentMasteryExp = (int)Game1.stats.Get("MasteryExp");
            int expToSet = totalSpentLevels <= 5
                ? Math.Max(MasteryTrackerMenu.getMasteryExpNeededForLevel(totalSpentLevels), Math.Min(currentMasteryExp, MasteryTrackerMenu.getMasteryExpNeededForLevel(5)))
                : MasteryTrackerMenu.getMasteryExpNeededForLevel(totalSpentLevels);
            Game1.stats.Set("MasteryExp", expToSet);
        }

        private static void clearAllProfessions()
        {
            if (Game1.player is null)
            {
                LogMonitor.Log("You need to load a save for the command to work");
                return;
            }

            Game1.player.professions.RemoveWhere(p => 0<=p && p <= 29);

            foreach (Skill s in MasterySkillsPage.skills.FindAll(s => 0<= s.Id && s.Id <= 4))
            {
                int level = s.getLevel();

                if (level >= 5)
                {
                    Game1.player.newLevels.Add(new Point(s.Id, 5));
                }
                if (level >= 10)
                {
                    Game1.player.newLevels.Add(new Point(s.Id, 10));
                }
            }
        }

        /// <summary>Save the data when the game is saved.</summary>
        private void OnSaving(object? sender, SavingEventArgs e)
        {
            Helper.Data.WriteSaveData("AlanBF.MasteryExtended", Data);
        }
    }
}
