using HarmonyLib;
using MasteryExtended.Compatibility.SpaceCore.Patches;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace MasteryExtended.Compatibility.SpaceCore
{
    internal static class SCLoader
    {
        internal static void Loader(IModHelper _1, Harmony harmony)
        {
            SpaceCorePatches(harmony);
        }

        /// <summary>Base patches for the mod.</summary>
        /// <param name="harmony">Harmony instance used to patch the game.</param>
        internal static void SpaceCorePatches(Harmony harmony)
        {
            /************************************
             * Farmer Experience & Mastery Gain *
             ************************************/
            // Cambia como se gana experiencia en SpaceCore
            harmony.Patch(
                original: AccessTools.Method("SpaceCore.Skills:AddExperience"),
                transpiler: new HarmonyMethod(typeof(SCSkillsPatch), nameof(SCSkillsPatch.AddExperienceTranspiler))
            );

            /******************
             * Register Skill *
             ******************/
            // Agrega el skill directo desde SpaceCore
            harmony.Patch(
                original: AccessTools.Method("SpaceCore.Skills:RegisterSkill"),
                postfix: new HarmonyMethod(typeof(SCSkillsPatch), nameof(SCSkillsPatch.RegisterSkillPostfix))
            );

            /*************
             * SkillPage *
             *************/
            // Cambia el formato de los skillBars, para no que no dibuje nada
            Type skillsPage = AccessTools.TypeByName("SpaceCore.Interface.NewSkillsPage");
            harmony.Patch(
                original: AccessTools.Constructor(skillsPage, [typeof(int), typeof(int), typeof(int), typeof(int)]),
                transpiler: new HarmonyMethod(typeof(SCNewSkillsPagePatch), nameof(SCNewSkillsPagePatch.ctorTranspiler))
            );

            // Cambia el ancho mostrado por la barra de maestría, modifica los números y agrega el nuevo dibujo de las profesiones
            harmony.Patch(
                original: AccessTools.Method("SpaceCore.Interface.NewSkillsPage:draw", [typeof(SpriteBatch)]),
                transpiler: new HarmonyMethod(typeof(SCNewSkillsPagePatch), nameof(SCNewSkillsPagePatch.drawTranspiler))
            );

            // Se asegura que se vean las profesiones al hacer Hover
            harmony.Patch(
                original: AccessTools.Method("SpaceCore.Interface.NewSkillsPage:performHoverAction"),
                transpiler: new HarmonyMethod(typeof(SCNewSkillsPagePatch), nameof(SCNewSkillsPagePatch.performHoverActionTranspiler))
            );
        }
    }
}