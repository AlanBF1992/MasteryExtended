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

namespace MasteryExtended
{
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod
    {
        /*********
        ** Fields
        **********/
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

        /*****************
        ** Public methods
        ******************/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();
            ModHelper = helper;
            LogMonitor = Monitor; // Solo aca empieza a existir

            // Insertar Parches necesarios
            var harmony = new Harmony(this.ModManifest.UniqueID);
            applyPatches(harmony);

            // Console command
            addConsoleCommands(helper);

            // Events
            helper.Events.GameLoop.GameLaunched += this.OnGameLaunched;
            helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
            helper.Events.GameLoop.Saving += this.OnSaving;
        }

        /*****************
        * Private methods
        ******************/
        /// <summary>Add commands to debug</summary>
        private static void addConsoleCommands(IModHelper helper)
        {
            helper.ConsoleCommands.Add(
                "masteryExtended_RecountUsed",
                "Recounts the player's Mastery Level Used.",
                (_, _) => recountUsedMasteryLevels());

            helper.ConsoleCommands.Add(
                "masteryExtended_ResetExp",
                "Sets the Mastery Exp to the minimum possible.",
                (_, _) => resetMasteryExp());

            helper.ConsoleCommands.Add(
                "masteryExtended_ResetProfessions",
                "Reset Vanilla Professions when you sleep.",
                (_, _) => { resetAllProfessionsVanilla(); recountUsedMasteryLevels(); });

            helper.ConsoleCommands.Add(
                "masteryExtended_AddMasteryLevel",
                "Add Mastery Levels. <value> is an optional argument.",
                addMasteryLevel);
        }

        /// <summary>Apply the patches for the mod.</summary>
        /// <param name="harmony">Harmony instance used to patch the game.</param>
        private void applyPatches(Harmony harmony)
        {
            /**********************
             * Farmer Mastery Gain
             **********************/
            // Cambia la forma en la que se calcula el nivel
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), "get_Level"),
                transpiler: new HarmonyMethod(typeof(FarmerPatch), nameof(FarmerPatch.LevelTranspiler))
            );

            // Cambia la forma en la que se entrega la info para el título
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.getTitle)),
                transpiler: new HarmonyMethod(typeof(FarmerPatch), nameof(FarmerPatch.getTitleTranspiler))
            );

            // Permite ganar Mastery desde que se llega al máximo de la primera profesión
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.gainExperience)),
                transpiler: new HarmonyMethod(typeof(FarmerPatch), nameof(FarmerPatch.gainExperienceTranspiler))
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

            // Modificaciones para eliminar el hover
            harmony.Patch(
                original: AccessTools.Method(typeof(SkillsPage), nameof(SkillsPage.draw), [typeof(SpriteBatch)]),
                prefix: new HarmonyMethod(typeof(SkillsPagePatch), nameof(SkillsPagePatch.drawPrefix))
            );

            // Modifica el nivel mostrado y devuelve el hover en la página de habilidades
            harmony.Patch(
                original: AccessTools.Method(typeof(SkillsPage), nameof(SkillsPage.draw), [typeof(SpriteBatch)]),
                postfix: new HarmonyMethod(typeof(SkillsPagePatch), nameof(SkillsPagePatch.drawPostfix))
            );

            /********************
             * Mastery Cave Door
             ********************/
            // Al hacer clic en la puerta, te permite acceder antes y te dice como
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.performAction), [typeof(string[]), typeof(Farmer), typeof(xTile.Dimensions.Location)]),
                //postfix: new HarmonyMethod(typeof(GameLocationPatch), nameof(GameLocationPatch.performActionPostfix))
                transpiler: new HarmonyMethod(typeof(GameLocationPatch), nameof(GameLocationPatch.performActionTranspiler))
            );

            /**********************************
             * Mastery Cave Pedestal & Pillars
             **********************************/
            // Modifica el menú del pedestal. Lo hace más alto y crea un botón
            harmony.Patch(
                original: AccessTools.Constructor(typeof(MasteryTrackerMenu), [typeof(int)]),
                postfix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.MasteryTrackerMenuPostfix))
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

            /***************
             * Mastery Cave
             ***************/
            // Update the Mastery Cave map when needed
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.MakeMapModifications)),
                postfix: new HarmonyMethod(typeof(GameLocationPatch), nameof(GameLocationPatch.MakeMapModificationsPostfix))
            );

            /***************
             * Dog Statue
             ***************/
             // Profession Forget Event
            harmony.Patch(
                original: AccessTools.Method(typeof(GameLocation), nameof(GameLocation.answerDialogueAction)),
                postfix: new HarmonyMethod(typeof(GameLocationPatch), nameof(GameLocationPatch.answerDialogueActionPostFix))
            );
        }

        /// <summary>GMCM Compat</summary>
        private void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
        {
            GMCMConfig();
        }

        /// <summary>Reads and applies the data when the save is loaded.</summary>
        [EventPriority(EventPriority.Low)]
        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            if (Context.IsMainPlayer)
            {
                // From here it checks the Data
                ModData? a = Helper.Data.ReadSaveData<ModData>("AlanBF.MasteryExtended");
                Data = a ?? new ModData();

                if (a is not null) return;

                recountUsedMasteryLevels();
                resetMasteryExp();
            } else
            {
                Data = new ModData
                {
                    claimedRewards = countClaimedPillars()
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

        public static void recountUsedMasteryLevels()
        {
            if (!Context.IsWorldReady)
            {
                LogMonitor.Log("You need to load a save to use this command.", LogLevel.Error);
                return;
            }

            int spentLevelsInProfessions = 0;

            foreach (Skill s in MasterySkillsPage.skills)
            {
                spentLevelsInProfessions += Math.Max(s.unlockedProfessions() - Math.Min((int)Math.Floor(s.getLevel() / 5f), 2), 0);
            }

            Data.claimedRewards = countClaimedPillars();
            int totalSpentLevels = countClaimedPillars() + spentLevelsInProfessions;

            Game1.stats.Set("masteryLevelsSpent", totalSpentLevels);
        }

        private static void resetMasteryExp()
        {
            if (!Context.IsWorldReady)
            {
                LogMonitor.Log("You need to load a save to use this command.", LogLevel.Error);
                return;
            }
            int spentLevelsInProfessions = 0;

            foreach (Skill s in MasterySkillsPage.skills)
            {
                spentLevelsInProfessions += Math.Max(s.unlockedProfessions() - Math.Min((int)Math.Floor(s.getLevel() / 5f), 2), 0);
            }

            int totalSpentLevels = countClaimedPillars() + spentLevelsInProfessions;

            int expToSet = MasteryTrackerMenu.getMasteryExpNeededForLevel(totalSpentLevels);

            Game1.stats.Set("MasteryExp", expToSet);
        }

        private static void resetAllProfessionsVanilla()
        {
            if (!Context.IsWorldReady)
            {
                LogMonitor.Log("You need to load a save to use this command.", LogLevel.Error);
                return;
            }

            Game1.player.professions.RemoveWhere(p => p is >= 0 and <= 29);

            foreach (Skill s in MasterySkillsPage.skills.FindAll(s => s.Id is >= 0 and <= 4))
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

        private static void addMasteryLevel(string command, string[] args)
        {
            if (!Context.IsWorldReady)
            {
                LogMonitor.Log("You need to load a save to use this command.", LogLevel.Error);
                return;
            }

            int levels = 1;
            if (args.Length > 0)
            {
                try
                {
                    levels = int.Parse(args[0]);
                }
                catch
                {
                    LogMonitor.Log("Parameter should be an integer", LogLevel.Error);
                    return;
                }
            }
            int currentLevel = MasteryTrackerMenu.getCurrentMasteryLevel();
            int spentLevels = (int)Game1.stats.Get("masteryLevelsSpent");
            int newLevel = (int)Utilities.EncloseNumber(spentLevels, currentLevel + levels, MaxMasteryPoints);
            int expToSet = MasteryTrackerMenu.getMasteryExpNeededForLevel(newLevel);

            Game1.stats.Set("MasteryExp", expToSet);
        }

        public static int countClaimedPillars()
        {
            List<string> checkRecipeList = ["Statue Of Blessings", "Heavy Furnace", "Challenge Bait", "Treasure Totem", "Anvil"];
            int spentLevelsInMasteryPillar = checkRecipeList.Count(x => Game1.player.craftingRecipes.ContainsKey(x));
            return spentLevelsInMasteryPillar;
        }

        private void GMCMConfig()
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = ModHelper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;
            // register mod
            configMenu.Register(
                mod: this.ModManifest,
                reset: () => Config = new ModConfig(),
                save: () => ModHelper.WriteConfig(Config)
            );

            // Mastery Exp per level
            configMenu.AddNumberOption(
                mod: ModManifest,
                getValue: () => Config.MasteryExpPerLevel,
                setValue: (value) => Config.MasteryExpPerLevel = value,
                name: () => "Mastery Experience for Level",
                tooltip: () => "Default: 30000",
                min: 15000,
                max: 50000,
                interval: 5000
            );

            // Mastery Exp per level
            configMenu.AddNumberOption(
                mod: ModManifest,
                getValue: () => Config.MasteryRequiredForCave,
                setValue: (value) => Config.MasteryRequiredForCave = value,
                name: () => "Mastery Levels for the cave",
                tooltip: () => "Default: 5",
                min: 0,
                max: 10,
                interval: 1
            );

            // Mastery Exp per level
            configMenu.AddBoolOption(
                mod: ModManifest,
                getValue: () => Config.ExtraRequiredProfession,
                setValue: (value) => Config.ExtraRequiredProfession = value,
                name: () => "3 Professions for pillars",
                tooltip: () => "Default: true"
            );
        }
    }
}
