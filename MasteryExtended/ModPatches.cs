using HarmonyLib;
using MasteryExtended.Patches;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace MasteryExtended
{
    internal static class ModPatches
    {
        /// <summary>Base patches for the mod.</summary>
        /// <param name="harmony">Harmony instance used to patch the game.</param>
        internal static void VanillaPatches(Harmony harmony)
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
    }
}
