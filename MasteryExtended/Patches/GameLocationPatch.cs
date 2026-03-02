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
                MethodInfo fullSkillsCompletedInfo = AccessTools.Method(typeof(GameLocationPatch), nameof(fullSkillsCompleted));
                MethodInfo fullSkillsRequiredInfo = AccessTools.Method(typeof(GameLocationPatch), nameof(fullSkillsRequired));

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
                //to:   if(currentMasteryLevel() >= masteryRequired() || fullSkillsCompleted() >= fullSkillsRequired())
                matcher
                    .Start()
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Ldc_I4_5),
                        new CodeMatch(OpCodes.Blt_S)
                    )
                    .ThrowIfNotMatch("Vanilla performAction: GetInMasteryCave IL code not found")
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Call, currentMasteryInfo),
                        new CodeInstruction(OpCodes.Call, masteryConfigInfo),
                        new CodeInstruction(OpCodes.Bge_S, getInsideCave)
                    )
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Call, fullSkillsCompletedInfo),
                        new CodeInstruction(OpCodes.Call, fullSkillsRequiredInfo)
                    )
                    .RemoveInstructions(2)
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

                int professionsRequired = ModEntry.Config.PillarsVsProfessions != "0" ? 2 : ModEntry.Config.RequiredProfessionForPillars;

                for (int which = 0; which < 5; which++)
                {
                    bool enoughProfessions = MasterySkillsPage.skills.Find(s => s.Id == which)!.unlockedProfessionsCount(0, 10) >= professionsRequired;

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

        // Extra Fishing tries with Specific Bait
        internal static IEnumerable<CodeInstruction> GetFishFromLocationDataTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);

                MethodInfo fishingTriesInfo = AccessTools.Method(typeof(GameLocationPatch), nameof(fishingTries));

                // from: for (int i = 0; i < 2; i++)
                // to:   for (int i = 0; i < fishingTries(); i++)
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldc_I4_2)
                    )
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldarg_3)
                    )
                    .SetInstruction(
                        new CodeInstruction(OpCodes.Call, fishingTriesInfo)
                    )
                ;

                // from: if (baitTargetFish == null || !(fish.QualifiedItemId != baitTargetFish) || targetedBaitTries >= 2)
                // to:   if (baitTargetFish == null || !(fish.QualifiedItemId != baitTargetFish) || targetedBaitTries >= fishingTries())
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldc_I4_2)
                    )
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldarg_3)
                    )
                    .SetInstruction(
                        new CodeInstruction(OpCodes.Call, fishingTriesInfo)
                    )
                ;

                return matcher.InstructionEnumeration();
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(GetFishFromLocationDataTranspiler)}:\n{ex}", LogLevel.Error);
                return instructions;
            }
        }

        /***********
         * METHODS *
         ***********/

        internal static int masteryRequired()
        {
            return (ModEntry.Config.SkillsVsMasteryPoints.Equals("1")) ? 999 : ModEntry.Config.MasteryRequiredForCave;
        }

        internal static void masteryCaveString()
        {
            int masteryLevel = MasteryTrackerMenu.getCurrentMasteryLevel();
            int divisor = ModEntry.MasteryCaveChanges();
            int vanillaSkillsReady = MasterySkillsPage.skills.Count(s => s.getLevel() / divisor >= 1 && s.Id >= 0 && s.Id <= 4);
            int allSkillsReady = MasterySkillsPage.skills.Count(s => s.getLevel() / divisor >= 1 && s.isVisible());
            int skillCheck = ModEntry.Config.IncludeCustomSkills ? allSkillsReady : vanillaSkillsReady;


            switch (ModEntry.Config.SkillsVsMasteryPoints)
            {
                case "0":
                    Game1.drawObjectDialogue([
                        Game1.content.LoadString("Strings\\1_6_Strings:MasteryCave", skillCheck).Replace(".","").Replace("/5", $"/{ModEntry.Config.SkillsRequiredForMasteryRoom}"),
                        Game1.content.LoadString("Strings\\UI:MasteryExtended_TrascendMortalKnowledge") + $" ({masteryLevel}/{masteryRequired()})"
                    ]);
                    break;
                case "1":
                    Game1.drawObjectDialogue(
                        Game1.content.LoadString("Strings\\1_6_Strings:MasteryCave", skillCheck).Replace(".", "").Replace("/5", $"/{ModEntry.Config.SkillsRequiredForMasteryRoom}")
                    );
                    break;
                case "2":
                    Game1.drawObjectDialogue(
                        Game1.content.LoadString("Strings\\UI:MasteryExtended_TrascendMortalKnowledgeOnly") + $" ({masteryLevel}/{masteryRequired()})"
                    );
                    break;
                case "3":
                    Game1.drawObjectDialogue([
                        Game1.content.LoadString("Strings\\1_6_Strings:MasteryCave", skillCheck).Replace(".","").Replace("/5", $"/{ModEntry.Config.SkillsRequiredForMasteryRoom}"),
                        Game1.content.LoadString("Strings\\UI:MasteryExtended_TrascendMortalKnowledgeTogether") + $" ({masteryLevel}/{masteryRequired()})"
                    ]);
                    break;
                default:
                    Game1.drawObjectDialogue(
                        Game1.content.LoadString("Strings\\1_6_Strings:MasteryCave", skillCheck).Replace("/5", $"/{ModEntry.Config.SkillsRequiredForMasteryRoom}")
                    );
                    break;
            }
        }

        internal static int fullSkillsCompleted()
        {
            int divisor = ModEntry.MasteryCaveChanges();
            int vanillaSkillsReady = MasterySkillsPage.skills.Count(s => s.getLevel() / divisor >= 1 && s.Id >= 0 && s.Id <= 4);
            int allSkillsReady = MasterySkillsPage.skills.Count(s => s.getLevel() / divisor >= 1 && s.isVisible());

            return ModEntry.Config.IncludeCustomSkills ? allSkillsReady : vanillaSkillsReady;
        }

        internal static int fullSkillsRequired()
        {
            return ModEntry.Config.SkillsRequiredForMasteryRoom;
        }

        internal static int fishingTries(Farmer who)
        {
            if (!who.modData.TryGetValue("ME_Specialist", out string value)) return 2;
            return bool.Parse(value) ? 5 : 2;
        }
    }
}
