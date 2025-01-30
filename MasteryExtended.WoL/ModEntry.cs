using HarmonyLib;
using MasteryExtended.Menu.Pages;
using MasteryExtended.WoL.Patches;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.Menus;

namespace MasteryExtended.WoL
{
    public class ModEntry : Mod
    {
        /*********
        ** Fields
        **********/
        /// <summary>The mod configuration from the player.</summary>
        internal static ModConfig Config = null!;
        /// <summary>Log info.</summary>
        public static IMonitor LogMonitor { get; private set; } = null!;
        /// <summary>Helper.</summary>
        public static IModHelper ModHelper { get; private set; } = null!;

        /*****************
        ** Public methods
        ******************/
        public override void Entry(IModHelper helper)
        {
            //Config = helper.ReadConfig<ModConfig>();
            ModHelper = helper;
            LogMonitor = Monitor; // Solo aca empieza a existir

            // Insertar Parches necesarios
            var harmony = new Harmony(this.ModManifest.UniqueID);
            applyPatches(harmony);

            MasteryExtended.ModEntry.MaxMasteryPoints += 20; //5 skill * 4 prestiges

            // SpaceCore solo hasta lvl 10
            helper.Events.GameLoop.SaveLoaded += this.fixExperienceCurve;
        }

        /// <summary>Makes SpaceCore Skills to be able to go up to level 10 only </summary>
        private void fixExperienceCurve(object? sender, SaveLoadedEventArgs e)
        {
            int[] ExperienceCurve =
                [
                    100,
                    380,
                    770,
                    1300,
                    2150,
                    3300,
                    4800,
                    6900,
                    10000,
                    15000,
                    int.MaxValue
                ];

            foreach (string skill in SpaceCore.Skills.GetSkillList())
            {
                SpaceCore.Skills.GetSkill(skill).ExperienceCurve = ExperienceCurve;
            }
        }

        private void applyPatches(Harmony harmony)
        {
            /*************************************
             * Vanilla Skill Experience with WoL *
             *************************************/
            harmony.Patch(
                original: AccessTools.Method("DaLion.Professions.Framework.Patchers.Prestige.FarmerGainExperiencePatcher:FarmerGainExperiencePrefix"),
                transpiler: new HarmonyMethod(typeof(GainExperiencePatch), nameof(GainExperiencePatch.FarmerGainExperiencePrefixTranspiler))
            );

            /********************
             * MAESTRÍAS CON WoL
             ********************/

            // No haga nada al cargar la partida
            harmony.Patch(
                original: AccessTools.Method("DaLion.Professions.Framework.Events.GameLoop.ProfessionSaveLoadedEvent:OnSaveLoadedImpl"),
                postfix: new HarmonyMethod(typeof(VanillaSkillPatch), nameof(VanillaSkillPatch.OnSaveLoadedImplPostfix))
            );

            /**************************
             * CAMBIAR DE COMBAT LIMIT
             **************************/

            // Accede al menú y reactiva el main button
            harmony.Patch(
                original: AccessTools.Constructor(typeof(MasteryTrackerMenu), [typeof(int)]),
                postfix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.MasteryTrackerMenuPostfix))
            );

            // Dibuja el botón
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.draw), [typeof(SpriteBatch)]),
                prefix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.drawPrefix)),
                postfix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.drawPostfix))
            );

            // Le da funcionalidad al botón
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.receiveLeftClick)),
                prefix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.receiveLeftClickPrefix))
            );

            // Highlight
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.performHoverAction)),
                postfix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.performHoverActionPostfix))
            );

            /***************
             * LVL 15 AL 20
             ***************/
            harmony.Patch(
                original: AccessTools.Constructor(typeof(MasteryProfessionsPage), [typeof(MasteryExtended.Skills.Skill)]),
                postfix: new HarmonyMethod(typeof(MasteryProfessionsPagePatch), nameof(MasteryProfessionsPagePatch.MasteryProfessionsPagePatchPostfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryProfessionsPage), nameof(MasteryProfessionsPage.draw), [typeof(SpriteBatch)]),
                postfix: new HarmonyMethod(typeof(MasteryProfessionsPagePatch), nameof(MasteryProfessionsPagePatch.drawPostfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryProfessionsPage), nameof(MasteryProfessionsPage.receiveLeftClick)),
                postfix: new HarmonyMethod(typeof(MasteryProfessionsPagePatch), nameof(MasteryProfessionsPagePatch.receiveLeftClickPostfix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryProfessionsPage), nameof(MasteryProfessionsPage.performHoverAction)),
                postfix: new HarmonyMethod(typeof(MasteryProfessionsPagePatch), nameof(MasteryProfessionsPagePatch.performHoverActionPostfix))
            );

            harmony.Patch(
                original: AccessTools.Method("DaLion.Professions.Framework.Patchers.Prestige.LevelUpMenuUpdatePatcher:LevelUpMenuUpdatePrefix"),
                transpiler: new HarmonyMethod(typeof(LevelUpMenuUpdatePatch), nameof(LevelUpMenuUpdatePatch.LevelUpMenuUpdatePrefixTranspiler))
            );

            //For now SpaceCore skills can only go to level 10
            harmony.Patch(
                original: AccessTools.Method("DaLion.Professions.Framework.Patchers.Prestige.Integration.SkillLevelUpMenuUpdatePatcher:SkillLevelUpMenuUpdatePrefix"),
                prefix: new HarmonyMethod(typeof(DaLionUnpatcher), nameof(DaLionUnpatcher.UnpatcherPrefix))
            );

            //Fix Message
            harmony.Patch(
                original: AccessTools.Method("DaLion.Professions.Framework.Patchers.Prestige.GameLocationPerformActionPatcher:GameLocationPerformActionPrefix"),
                prefix: new HarmonyMethod(typeof(DaLionUnpatcher), nameof(DaLionUnpatcher.UnpatcherPrefix))
            );
        }
    }
}
