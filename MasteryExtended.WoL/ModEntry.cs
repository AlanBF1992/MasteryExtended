using HarmonyLib;
using MasteryExtended.Skills.Professions;
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
        /// <summary> Professions to add to WoL Data</summary>
        public static List<int> ProfessionsToSave { get; } = new();

        /*****************
        ** Public methods
        ******************/
        public override void Entry(IModHelper helper)
        {
            Config = helper.ReadConfig<ModConfig>();
            ModHelper = helper;
            LogMonitor = Monitor; // Solo aca empieza a existir

            // Insertar Parches necesarios
            var harmony = new Harmony(this.ModManifest.UniqueID);
            applyPatches(harmony);

            // Events
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
        }

        private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
        {
            ProfessionsToSave.Clear();
        }

        private void applyPatches(Harmony harmony)
        {
            // Marca una profession para guardarla luego
            harmony.Patch(
                original: AccessTools.Method(typeof(Profession), nameof(Profession.AddProfessionToPlayer)),
                postfix: new HarmonyMethod(typeof(ProfessionPatch), nameof(ProfessionPatch.AddProfessionToPlayerPostfix))
            );
            // Agrega las profesiones que no guardó WoL
            harmony.Patch(
                original: AccessTools.Method("DaLion.Professions.Framework.Events.GameLoop.ProfessionSavingEvent:OnSavingImpl"),
                postfix: new HarmonyMethod(typeof(ProfessionSavingEventPatch), nameof(ProfessionSavingEventPatch.OnSavingImplPostfix))
            );
            // Accede al menú y reactiva el main button
            harmony.Patch(
                original: AccessTools.Constructor(typeof(MasteryTrackerMenu), new Type[] { typeof(int) }),
                postfix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.MasteryTrackerMenuPostfix))
            );

            // Dibuja el botón
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.draw), new Type[] { typeof(SpriteBatch) }),
                prefix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.drawPrefix)),
                postfix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.drawPostfix))
            );

            // Le da funcionalidad al botón
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.receiveLeftClick)),
                prefix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.receiveLeftClickPrefix))
            );

            // Permite que el botón muestre el porqué no se puede aclamar
            harmony.Patch(
                original: AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.performHoverAction)),
                postfix: new HarmonyMethod(typeof(MasteryTrackerMenuPatch), nameof(MasteryTrackerMenuPatch.performHoverActionPostfix))
            );
        }
    }
}
