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
            // Changes how experience is gained in SpaceCore
            harmony.Patch(
                original: AccessTools.Method("SpaceCore.Skills:AddExperience"),
                transpiler: new HarmonyMethod(typeof(SCSkillsPatch), nameof(SCSkillsPatch.AddExperienceTranspiler))
            );

            /******************
             * Register Skill *
             ******************/
            // Adds the skill directly from SpaceCore
            harmony.Patch(
                original: AccessTools.Method("SpaceCore.Skills:RegisterSkill"),
                postfix: new HarmonyMethod(typeof(SCSkillsPatch), nameof(SCSkillsPatch.RegisterSkillPostfix))
            );

            /*************
             * SkillPage *
             *************/
            // Changes the skillBars format to prevent drawing
            Type skillsPage = AccessTools.TypeByName("SpaceCore.Interface.NewSkillsPage");
            harmony.Patch(
                original: AccessTools.Constructor(skillsPage, [typeof(int), typeof(int), typeof(int), typeof(int)]),
                transpiler: new HarmonyMethod(typeof(SCNewSkillsPagePatch), nameof(SCNewSkillsPagePatch.ctorTranspiler))
            );

            // Changes the Mastery Bar width, modifies numbers, and adds new profession drawing
            harmony.Patch(
                original: AccessTools.Method("SpaceCore.Interface.NewSkillsPage:draw", [typeof(SpriteBatch)]),
                transpiler: new HarmonyMethod(typeof(SCNewSkillsPagePatch), nameof(SCNewSkillsPagePatch.drawTranspiler))
            );

            // Makes sure professions are shown when hovering
            harmony.Patch(
                original: AccessTools.Method("SpaceCore.Interface.NewSkillsPage:performHoverAction"),
                transpiler: new HarmonyMethod(typeof(SCNewSkillsPagePatch), nameof(SCNewSkillsPagePatch.performHoverActionTranspiler))
            );
        }
    }
}