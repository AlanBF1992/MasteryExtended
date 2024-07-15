using DaLion.Professions;
using HarmonyLib;
using MasteryExtended.Menu.Pages;
using MasteryExtended.Skills.Professions;
using MasteryExtended.WoL.Patches;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
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
        }

        private void applyPatches(Harmony harmony)
        {
            /********************
             * MAESTRIAS CON WoL
             ********************/

            // Al agregar una profesión con maestría, la agrega a WoL
            harmony.Patch(
                original: AccessTools.Method(typeof(Profession), nameof(Profession.AddProfessionToPlayer)),
                postfix: new HarmonyMethod(typeof(ProfessionPatch), nameof(ProfessionPatch.AddProfessionToPlayerPostfix))
            );

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
                prefix: new HarmonyMethod(typeof(DaLionUnpatcher), nameof(DaLionUnpatcher.LevelUpMenuUpdateUnpatcherPrefix))
            );

            harmony.Patch(
                original: AccessTools.Method(typeof(LevelUpMenu), nameof(LevelUpMenu.update)),
                prefix: new HarmonyMethod(typeof(LevelUpMenuUpdatePatch), nameof(LevelUpMenuUpdatePatch.LevelUpMenuUpdatePrefix))
            );
        }
    }
}
