using HarmonyLib;
using MasteryExtended.Menu.Pages;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Reflection;
using System.Reflection.Emit;

namespace MasteryExtended.Patches
{
    internal static class GameLocationPatch
    {
        internal readonly static IMonitor LogMonitor = ModEntry.LogMonitor;

        /***********
         * PATCHES *
         ***********/

        internal static IEnumerable<CodeInstruction> performActionTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            try
            {
                CodeMatcher matcher = new(instructions, generator);

                MethodInfo masteryConfigInfo = AccessTools.Method(typeof(GameLocationPatch), nameof(masteryRequired));
                MethodInfo currentMasteryInfo = AccessTools.Method(typeof(MasteryTrackerMenu), nameof(MasteryTrackerMenu.getCurrentMasteryLevel));
                MethodInfo dialogueStringInfo = AccessTools.Method(typeof(GameLocationPatch), nameof(masteryCaveString));

                //from: Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:MasteryCave", num2));
                //to:   Game1.drawObjectDialogue(masteryCaveString());
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldstr, "doorClose")
                    )
                    .ThrowIfNotMatch("Vanilla performAction: Label Out code not found")
                    .CreateLabel(out Label getInsideCave)
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldsfld),
                        new CodeMatch(OpCodes.Ldstr, "Strings\\1_6_Strings:MasteryCave"),
                        new CodeMatch(OpCodes.Ldloc_S)
                    )
                    .ThrowIfNotMatch("Vanilla performAction: Label String code not found")
                ;

                Label stringLabel = matcher.Labels[0];

                matcher
                    .RemoveInstructions(6)
                    .Insert(
                        new CodeInstruction(OpCodes.Call, dialogueStringInfo)
                    )
                    .AddLabels([stringLabel])
                ;

                //from: if(int2 >= 5)
                //to:   if(currentMasteryLevel >= masteryRequired || int2 >= 5)
                matcher
                    .Start()
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Ldc_I4_5),
                        new CodeMatch(OpCodes.Blt_S)
                    )
                    .ThrowIfNotMatch("Vanilla performAction: GetInMasteryCave IL code not found")
                    .Insert(
                        new CodeInstruction(OpCodes.Call, currentMasteryInfo),
                        new CodeInstruction(OpCodes.Call, masteryConfigInfo),
                        new CodeInstruction(OpCodes.Bge_S, getInsideCave)
                    )
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(performActionTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        // Mostrar bien cuál pilar se puede obtener.
        internal static void MakeMapModificationsPostfix(GameLocation __instance)
        {
            try
            {
                if (__instance.Name != "MasteryCave") return;

                int professionsRequired = ModEntry.Config.PillarsVsProfessions != "Professions required for Pillars" ? 2 : ModEntry.Config.RequiredProfessionForPillars;

                for (int which = 0; which < 5; which++)
                {
                    bool enoughProfessions = MasterySkillsPage.skills.Find(s => s.Id == which)!.unlockedProfessionsCount(0,10) >= professionsRequired;

                    Game1.stats.Get("MasteryExp");
                    bool freeLevel = MasteryTrackerMenu.getCurrentMasteryLevel() > (int)Game1.stats.Get("masteryLevelsSpent");

                    if (!enoughProfessions || !freeLevel)
                    {
                        Game1.currentLocation.removeTemporarySpritesWithID(8765 + which);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(MakeMapModificationsPostfix)}:\n{ex}", LogLevel.Error);
            }
        }

       // Profession Forget After Recount Used Mastery Levels
        internal static void answerDialogueActionPostFix(string questionAndAnswer)
        {
            if (!questionAndAnswer.StartsWith("professionForget_")) return;

            ModCommands.recountUsedMasteryLevels();
        }

        /***********
         * METHODS *
         ***********/

        internal static int masteryRequired()
        {
            return (ModEntry.Config.MasteryCaveAlternateOpening) ? ModEntry.Config.MasteryRequiredForCave : 999;
        }

        internal static void masteryCaveString()
        {
            int masteryLevel = MasteryTrackerMenu.getCurrentMasteryLevel();
            int divisor = ModEntry.MasteryCaveChanges() ? 20 : 10;
            int fullSkills = Math.Min(Game1.player.farmingLevel.Value/divisor, 1)
                             + Math.Min(Game1.player.fishingLevel.Value/divisor, 1)
                             + Math.Min(Game1.player.foragingLevel.Value/divisor, 1)
                             + Math.Min(Game1.player.combatLevel.Value/divisor, 1)
                             + Math.Min(Game1.player.miningLevel.Value/divisor, 1);

            if (ModEntry.Config.MasteryCaveAlternateOpening)
            {
                Game1.drawObjectDialogue([
                    Game1.content.LoadString("Strings\\1_6_Strings:MasteryCave", fullSkills).Replace(".",""),
                    Game1.content.LoadString("Strings\\UI:MasteryExtended_TrascendMortalKnowledge") + $" ({masteryLevel}/{masteryRequired()})"
                ]);
                return;
            }
            Game1.drawObjectDialogue(
                Game1.content.LoadString("Strings\\1_6_Strings:MasteryCave", fullSkills)
            );
        }
    }
}
