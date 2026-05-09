using HarmonyLib;
using MasteryExtended.Menu.Pages;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System.Reflection;
using System.Reflection.Emit;
using xTile.Dimensions;

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

                // From: Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\1_6_Strings:MasteryCave", num2));
                // To:   Game1.drawObjectDialogue(masteryCaveString());
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldstr, "doorClose")
                    )
                    .ThrowIfNotMatch("GameLocationPatch.performActionTranspiler: IL code 1 not found")
                    .CreateLabel(out Label getInsideCave)
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldsfld),
                        new CodeMatch(OpCodes.Ldstr, "Strings\\1_6_Strings:MasteryCave"),
                        new CodeMatch(OpCodes.Ldloc_S)
                    )
                    .ThrowIfNotMatch("GameLocationPatch.performActionTranspiler: IL code 2 not found")
                ;

                Label stringLabel = matcher.Labels[0];

                matcher
                    .RemoveInstructions(6)
                    .Insert(
                        new CodeInstruction(OpCodes.Call, dialogueStringInfo)
                    )
                    .AddLabels([stringLabel])
                ;

                // From: if(int2 >= 5)
                // To:   if(currentMasteryLevel() >= masteryRequired() || fullSkillsCompleted() >= fullSkillsRequired())
                matcher
                    .Start()
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldloc_S),
                        new CodeMatch(OpCodes.Ldc_I4_5),
                        new CodeMatch(OpCodes.Blt_S)
                    )
                    .ThrowIfNotMatch("GameLocationPatch.performActionTranspiler: IL code 3 not found")
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

        internal static void MakeMapModificationsPostfix(GameLocation __instance)
        {
            try
            {
                if (__instance.Name != "MasteryCave") return;

                int professionsRequired = ModEntry.Config.PillarsVsProfessions != PillarsVsProfessionsOption.Professions ? 2 : ModEntry.Config.RequiredProfessionForPillars;

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

        internal static bool performActionPrefix(GameLocation __instance, string[] action, Farmer who, Location tileLocation, ref bool __result)
        {
            if (__instance.ShouldIgnoreAction(action, who, tileLocation)
                || !ArgUtility.TryGet(action, 0, out var actionType, out var _, allowBlank: true)
                || !who.IsLocalPlayer
                || !actionType.Equals("DogStatue"))
            {
                return true;
            }

            List<Response> mainDialogueOptions =
            [
                new Response("Powers", Game1.content.LoadString("Strings\\Locations:MasteryExtended_DogStatueMenuQuestionMainOptionPower")),
                new Response("Reset",  Game1.content.LoadString("Strings\\Locations:MasteryExtended_DogStatueMenuQuestionMainOptionReset")),
                new Response("Cancel", Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueCancel")),
            ];

            string displayed_text = Game1.content.LoadString("Strings\\Locations:Sewer_DogStatue");
            displayed_text = displayed_text[..displayed_text.IndexOf('^')] + "^";
            displayed_text += Game1.content.LoadString("Strings\\Locations:MasteryExtended_DogStatueMenuQuestionMainDialogue");
            __instance.createQuestionDialogue(displayed_text, mainDialogueOptions.ToArray(), "dogStatue");
            __result = true;
            return false;
        }

        internal static void answerDialogueActionPostFix(GameLocation __instance, string questionAndAnswer)
        {
            Farmer who = Game1.player;

            switch (questionAndAnswer)
            {
                case "dogStatue_Reset":
                    if (Enumerable.Range(0, 5).Any(GameLocation.canRespec))
                    {
                        __instance.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatue"), __instance.createYesNoResponses(), "dogStatue");
                        break;
                    }
                    string text1 = Game1.content.LoadString("Strings\\Locations:Sewer_DogStatue");
                    text1 = text1[..text1.LastIndexOf('^')];
                    Game1.drawObjectDialogue(text1);
                    break;

                case "dogStatue_Powers":
                case string s when s.StartsWith("dogStatue_PowersAdd_") && s.EndsWith("No"):

                    bool enoughMasteryPoints = MasteryTrackerMenu.getCurrentMasteryLevel() > (int)Game1.stats.Get("masteryLevelsSpent");
                    List<Response> powerOptions = [];

                    if (enoughMasteryPoints)
                    {
                        var skillPowers = new (int skillId, string powerId, string label, Func<Farmer, bool> hasPower)[]
                        {
                            (0, "Reaper", Game1.content.LoadString("Strings\\UI:MasteryExtended_ReaperName"), MeleeWeaponPatch.isFarmerReaper),
                            (3, "Mason", Game1.content.LoadString("Strings\\UI:MasteryExtended_MasonName"), MineShaftPatch.isFarmerMason),
                            (2, "Woodlander", Game1.content.LoadString("Strings\\UI:MasteryExtended_WoodlanderName"), TreePatch.isFarmerWoodlander),
                            (1, "Baitbinder", Game1.content.LoadString("Strings\\UI:MasteryExtended_BaitbinderName"), CrabPotPatch.isFarmerBaitbinder),
                            (4, "Runesmith", Game1.content.LoadString("Strings\\UI:MasteryExtended_RunesmithName"), MeleeWeaponPatch.isFarmerRunesmith)
                        };

                        foreach (var (skill, id, label, hasPower) in skillPowers)
                        {
                            if (who.GetUnmodifiedSkillLevel(skill) >= 10 && !hasPower(who))
                            {
                                powerOptions.Add(new Response(id, label));
                            }
                        }

                        if (who.Level >= 20 && !FarmerPatch.isFarmerAttractive(who))
                        {
                            powerOptions.Add(new Response("Attractive", Game1.content.LoadString("Strings\\UI:MasteryExtended_AttractiveName")));
                        }
                    }

                    if (powerOptions.Count == 0)
                    {
                        string text2 = Game1.content.LoadString("Strings\\Locations:Sewer_DogStatue");
                        text2 = text2[..text2.IndexOf('^')] + "^";
                        if (enoughMasteryPoints)
                        {
                            text2 += Game1.content.LoadString("Strings\\Locations:MasteryExtended_DogStatueMenuGotAllPowers");
                        }
                        else
                        {
                            text2 += Game1.content.LoadString("Strings\\Locations:MasteryExtended_DogStatueMenuNeedMoreMasteryOrLevels");
                        }
                        Game1.drawObjectDialogue(text2);
                        break;
                    }

                    powerOptions.Add(new Response("Cancel", Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueCancel")));

                    string text3 = Game1.content.LoadString("Strings\\Locations:Sewer_DogStatue");
                    text3 = text3[..text3.IndexOf('^')] + "^";
                    text3 += Game1.content.LoadString("Strings\\Locations:MasteryExtended_DogStatueMenuQuestionWhatPowerDialogue");

                    __instance.createQuestionDialogue(text3, powerOptions.ToArray(), "dogStatue_PowersInfo");
                    break;

                // Show the information about the power
                case string s when s.StartsWith("dogStatue_PowersInfo_"):
                    string powerId = s["dogStatue_PowersInfo_".Length..];

                    if (powerId == "Cancel") break;

                    PowerInfo power = PowerInfo.PowerList.First(x => x.Id.EndsWith(powerId));
                    string powerName = Game1.content.LoadString(power.DisplayNamePath);
                    string powerDesc = Game1.content.LoadString(power.PowerDescriptionPath, power.GetSubstitutions());

                    string text4 = "-" + powerName + "-^";
                    text4 += powerDesc + "^";
                    text4 += Game1.content.LoadString("Strings\\Locations:MasteryExtended_DogStatueMenuQuestionUnlockDialogue");

                    __instance.createQuestionDialogue(text4, __instance.createYesNoResponses(), $"dogStatue_PowersAdd_{powerId}");
                    break;

                // Add the power
                case string s when s.StartsWith("dogStatue_PowersAdd_") && s.EndsWith("Yes"):
                    Game1.stats.Increment("masteryLevelsSpent");

                    string powerId2 = s.Replace("dogStatue_PowersAdd_", "").Replace("_Yes", "");

                    who.modData.Add($"{ModEntry.ModManifest.UniqueID}/ExtraMastery/{powerId2}", "true");

                    Game1.drawObjectDialogue(Game1.content.LoadString("Strings\\Locations:Sewer_DogStatueFinished"));

                    if (powerId2.Equals("Woodlander"))
                    {
                        ModEntry.ModHelper.GameContent.InvalidateCache("Data\\CraftingRecipes");
                    }

                    DelayedAction.playSoundAfterDelay("dog_bark", 300);
                    DelayedAction.playSoundAfterDelay("dog_bark", 900);
                    break;

                case string s when s.StartsWith("professionForget_"):
                    ModCommands.recountUsedMasteryLevels();
                    break;
            }
        }

        internal static IEnumerable<CodeInstruction> GetFishFromLocationDataTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            try
            {
                CodeMatcher matcher = new(instructions);

                MethodInfo fishingTriesInfo = AccessTools.Method(typeof(GameLocationPatch), nameof(fishingTries));

                // From: for (int i = 0; i < 2; i++)
                // To:   for (int i = 0; i < fishingTries(); i++)
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldc_I4_2)
                    )
                    .ThrowIfNotMatch("GameLocationPatch.GetFishFromLocationDataTranspiler: IL code 1 not found")
                    .InsertAndAdvance(
                        new CodeInstruction(OpCodes.Ldarg_3)
                    )
                    .SetInstruction(
                        new CodeInstruction(OpCodes.Call, fishingTriesInfo)
                    )
                ;

                // From: if (baitTargetFish == null || !(fish.QualifiedItemId != baitTargetFish) || targetedBaitTries >= 2)
                // To:   if (baitTargetFish == null || !(fish.QualifiedItemId != baitTargetFish) || targetedBaitTries >= fishingTries())
                matcher
                    .MatchStartForward(
                        new CodeMatch(OpCodes.Ldc_I4_2)
                    )
                    .ThrowIfNotMatch("GameLocationPatch.GetFishFromLocationDataTranspiler: IL code 2 not found")
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
            return (ModEntry.Config.SkillsVsMasteryPoints == SkillsVsMasteryPointsOption.Skill) ? 999 : ModEntry.Config.MasteryRequiredForCave;
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
                case SkillsVsMasteryPointsOption.SkillOrMastery:
                    Game1.drawObjectDialogue([
                        Game1.content.LoadString("Strings\\1_6_Strings:MasteryCave", skillCheck).Replace(".","").Replace("/5", $"/{ModEntry.Config.SkillsRequiredForMasteryRoom}"),
                        Game1.content.LoadString("Strings\\UI:MasteryExtended_TrascendMortalKnowledge") + $" ({masteryLevel}/{masteryRequired()})"
                    ]);
                    break;
                case SkillsVsMasteryPointsOption.Skill:
                    Game1.drawObjectDialogue(
                        Game1.content.LoadString("Strings\\1_6_Strings:MasteryCave", skillCheck).Replace(".", "").Replace("/5", $"/{ModEntry.Config.SkillsRequiredForMasteryRoom}")
                    );
                    break;
                case SkillsVsMasteryPointsOption.Mastery:
                    Game1.drawObjectDialogue(
                        Game1.content.LoadString("Strings\\UI:MasteryExtended_TrascendMortalKnowledgeOnly") + $" ({masteryLevel}/{masteryRequired()})"
                    );
                    break;
                case SkillsVsMasteryPointsOption.SkillAndMastery:
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
            if (!CrabPotPatch.isFarmerBaitbinder(who)) return 2;
            return 5;
        }
    }
}
