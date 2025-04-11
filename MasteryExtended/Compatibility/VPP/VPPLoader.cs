using HarmonyLib;
using MasteryExtended.Compatibility.GMCM;
using MasteryExtended.Compatibility.VPP.Patches;
using MasteryExtended.Menu.Pages;
using MasteryExtended.Skills;
using MasteryExtended.Skills.Professions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace MasteryExtended.Compatibility.VPP
{
    internal static class VPPLoader
    {
        internal static void Loader(IModHelper helper, Harmony harmony)
        {
            VPPPatches(harmony);

            helper.Events.GameLoop.GameLaunched += checkVPPAPI;
            helper.Events.GameLoop.GameLaunched += addVPPProfessions;
            helper.Events.GameLoop.GameLaunched += GMCMConfigVPP;
        }

        private static void checkVPPAPI(object? sender, GameLaunchedEventArgs e)
        {
            var vppAPI = ModEntry.ModHelper.ModRegistry.GetApi<IVanillaPlusProfessions>("KediDili.VanillaPlusProfessions")!;
            ModEntry.MasteryCaveChanges = () => vppAPI.MasteryCaveChanges;
        }

        /// <summary>Add Lvl 15 and 20 Professions</summary>
        private static void addVPPProfessions(object? sender, GameLaunchedEventArgs e)
        {
            var professionDict = (System.Collections.IDictionary)AccessTools.Field("VanillaPlusProfessions.ModEntry:Professions").GetValue(null)!;

            Texture2D professionIcons = Game1.content.Load<Texture2D>("VanillaPlusProfessions\\ProfessionIcons");

            foreach (var key in professionDict.Keys)
            {
                dynamic VPPProfession = professionDict[key]!;

                Skill skill = MasterySkillsPage.skills.Find(s => s.Id == VPPProfession.Skill)!;

                Profession? requiredProfession = VPPProfession.LevelRequirement == 20 ? null : skill.Professions.Find(p => p.Id == VPPProfession.Requires);

                Profession toAdd = new(id: VPPProfession.ID,
                                       name: (Func<string>)(() => Game1.content.LoadString("Strings\\UI:LevelUp_ProfessionName_" + key)),
                                       description: (Func<string>)(() => Game1.content.LoadString("Strings\\UI:LevelUp_ProfessionDescription_" + key)),
                                       levelRequired: VPPProfession.LevelRequirement,
                                       requiredProfession: requiredProfession,
                                       textureSource: (Func<Texture2D>)(() => professionIcons),
                                       textureBounds: new Rectangle((VPPProfession.ID - 467830) % 6 * 16, (VPPProfession.ID - 467830) / 6 * 16, 16, 16));

                skill.Professions.Add(toAdd);
            }
            ModEntry.MaxMasteryLevels += 40;
        }

        /// <summary>GMCM Compat VPP</summary>
        private static void GMCMConfigVPP(object? _1, GameLaunchedEventArgs _2)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = ModEntry.ModHelper.ModRegistry.GetApi<IGMCMApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // Add Wol Section
            configMenu.AddSectionTitle(
                mod: ModEntry.ModManifest,
                text: () => "VPP Compat Options"
            );

            // Percentage of Exp to Mastery
            configMenu.AddNumberOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.MasteryPercentage,
                setValue: (value) => ModEntry.Config.MasteryPercentage = value,
                name: () => "Experience Percent for Mastery",
                tooltip: () => "Between Level 11 and 20. Default: 20",
                min: 0,
                max: 100,
                interval: 1
            );

            configMenu.AddNumberOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.Lvl10ProfessionsRequired,
                setValue: (value) => ModEntry.Config.Lvl10ProfessionsRequired = value,
                name: () => "Lvl 10 for a Lvl 15",
                tooltip: () => "Default: 2",
                min: 1,
                max: 4,
                interval: 1
            );

            configMenu.AddNumberOption(
                mod: ModEntry.ModManifest,
                getValue: () => ModEntry.Config.Lvl15ProfessionsRequired,
                setValue: (value) => ModEntry.Config.Lvl15ProfessionsRequired = value,
                name: () => "Lvl 15 for a Lvl 20",
                tooltip: () => "Default: 4",
                min: 1,
                max: 8,
                interval: 1
            );
        }

        /// <summary>All the patches to make VPP work</summary>
        private static void VPPPatches(Harmony harmony)
        {
            /***********************
             * CHANGE MASTERY GAIN *
             ***********************/
            // Add Full Mastery Exp when lvl 20
            harmony.Patch(
                original: AccessTools.Method("MasteryExtended.Patches.FarmerPatch:ShouldGainMasteryExp"),
                transpiler: new HarmonyMethod(typeof(FarmerPatch), nameof(FarmerPatch.ShouldGainMasteryExpTranspiler))
            );
            // Add Partial Mastery Exp when lvl 10 to 19
            harmony.Patch(
                original: AccessTools.Method(typeof(Farmer), nameof(Farmer.gainExperience)),
                transpiler: new HarmonyMethod(typeof(FarmerPatch), nameof(FarmerPatch.gainExperienceTranspiler))
            );

            /***************
             * CHANGE BASE *
             ***************/
            // Add Next Page button to MasteryProfessionPage
            harmony.Patch(
                original: AccessTools.Constructor(typeof(MasteryProfessionsPage), [typeof(Skill)]),
                postfix: new HarmonyMethod(typeof(MasteryProfessionsPagePatch), nameof(MasteryProfessionsPagePatch.CtorPostfix))
            );

            // Add leftClick to Next Page
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryProfessionsPage), nameof(MasteryProfessionsPage.receiveLeftClick)),
                prefix: new HarmonyMethod(typeof(MasteryProfessionsPagePatch), nameof(MasteryProfessionsPagePatch.receiveLeftClickPrefix))
            );

            // Add update to Next Page
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryProfessionsPage), nameof(MasteryProfessionsPage.update)),
                prefix: new HarmonyMethod(typeof(MasteryProfessionsPagePatch), nameof(MasteryProfessionsPagePatch.updatePrefix))
            );

            /***********************
             * OTHERS *
             ***********************/
            // Stop the hover
            harmony.Patch(
                original: AccessTools.Method("VanillaPlusProfessions.DisplayHandler:OnRenderedActiveMenu"),
                transpiler: new HarmonyMethod(typeof(DisplayHandlerPatch), nameof(DisplayHandlerPatch.OnRenderedActiveMenuTranspiler))
            );

            // Rename MyCustomSkillBars
            harmony.Patch(
                original: AccessTools.Method("VanillaPlusProfessions.ModEntry:OnButtonPressed"),
                transpiler: new HarmonyMethod(typeof(ModEntryPatcher), nameof(ModEntryPatcher.OnButtonPressedTranspiler))
            );

            // Don't let VPP handle things. I'm the handler now
            harmony.Patch(
                original: AccessTools.Method("VanillaPlusProfessions.DisplayHandler:HandleSkillPage"),
                transpiler: new HarmonyMethod(typeof(DisplayHandlerPatch), nameof(DisplayHandlerPatch.HandleSkillPageTranspiler))
            );
        }
    }
}
