using HarmonyLib;
using MasteryExtended.Menu.Pages;
using MasteryExtended.Skills;
using MasteryExtended.Skills.Professions;
using MasteryExtended.VPP.Patches;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace MasteryExtended.VPP
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
        //
        public static Func<bool> MasteryCaveChanges { get; private set; } = null!;

        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();
            ModHelper = helper;
            LogMonitor = Monitor; // Solo aca empieza a existir

            // Insertar Parches necesarios
            var harmony = new Harmony(this.ModManifest.UniqueID);
            applyPatches(harmony);

            helper.Events.GameLoop.GameLaunched += this.checkVPPAPI;
            helper.Events.GameLoop.GameLaunched += this.addVPPProfessions;
            helper.Events.GameLoop.GameLaunched += this.GMCMConfig;
        }

        private void checkVPPAPI(object? sender, GameLaunchedEventArgs e)
        {
            var vppAPI = ModHelper.ModRegistry.GetApi<IVanillaPlusProfessions>("KediDili.VanillaPlusProfessions")!;
            MasteryCaveChanges = () => vppAPI.MasteryCaveChanges;
        }

        private void addVPPProfessions(object? sender, GameLaunchedEventArgs e)
        {
            var professionDict = (Dictionary<string, VanillaPlusProfessions.Profession>)AccessTools.Field("VanillaPlusProfessions.ModEntry:Professions").GetValue(null)!;

            Texture2D professionIcons = Game1.content.Load<Texture2D>("VanillaPlusProfessions\\ProfessionIcons");

            foreach (var key in professionDict.Keys)
            {
                VanillaPlusProfessions.Profession VPPProfession = professionDict[key];

                Skill skill = MasterySkillsPage.skills.Find(s => s.Id == VPPProfession.Skill)!;

                Profession? requiredProfession = VPPProfession.LevelRequirement == 20 ? null : skill.Professions.Find(p => p.Id == VPPProfession.Requires);

                Profession toAdd = new(
                    VPPProfession.ID,
                    () => Game1.content.LoadString("Strings\\UI:LevelUp_ProfessionName_" + key),
                    VPPProfession.LevelRequirement,
                    () => Game1.content.LoadString("Strings\\UI:LevelUp_ProfessionDescription_" + key),
                    professionIcons,
                    new((VPPProfession.ID - 467830) % 6 * 16, (VPPProfession.ID - 467830) / 6 * 16, 16, 16),
                    requiredProfession
                );

                skill.Professions.Add(toAdd);
            }
            MasteryExtended.ModEntry.MaxMasteryPoints += 40;
        }

        private void applyPatches(Harmony harmony)
        {
            // Add Mastery Exp when lvl 10 or 20
            harmony.Patch(
                original: AccessTools.Method("MasteryExtended.Patches.FarmerPatch:ShouldGainMasteryExp"),
                transpiler: new HarmonyMethod(typeof(FarmerPatch), nameof(FarmerPatch.ShouldGainMasteryExpTranspiler))
            );

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
        }

        private void GMCMConfig(object? _1, GameLaunchedEventArgs _2)
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

            configMenu.AddNumberOption(
                mod: ModManifest,
                getValue: () => Config.Lvl10ProfessionsRequired,
                setValue: (value) => Config.Lvl10ProfessionsRequired = value,
                name: () => "Lvl 10 for a Lvl 15",
                tooltip: () => "Default: 2",
                min: 1,
                max: 4,
                interval: 1
            );

            configMenu.AddNumberOption(
                mod: ModManifest,
                getValue: () => Config.Lvl15ProfessionsRequired,
                setValue: (value) => Config.Lvl15ProfessionsRequired = value,
                name: () => "Lvl 15 for a Lvl 20",
                tooltip: () => "Default: 4",
                min: 1,
                max: 8,
                interval: 1
            );
        }
    }
}
