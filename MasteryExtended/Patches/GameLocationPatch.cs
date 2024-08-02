using StardewModdingAPI;
using StardewValley;
using xTile.Dimensions;
using StardewValley.Menus;
using MasteryExtended.Menu.Pages;

namespace MasteryExtended.Patches
{
    internal static class GameLocationPatch
    {
        internal static IMonitor LogMonitor = ModEntry.LogMonitor;

        // Acceder a la cueva antes
        internal static void performActionPostfix(GameLocation __instance, string[] action, Farmer who, Location tileLocation, ref bool __result)
        {
            try
            {
                if (__instance.ShouldIgnoreAction(action, who, tileLocation))
                {
                    __result = false;
                }
                if (!ArgUtility.TryGet(action, 0, out var actionType, out var error))
                {
                    __result = false;
                }
                if (who.IsLocalPlayer)
                {
                    if (actionType == "MasteryRoom")
                    {
                        int masteryLevel = MasteryTrackerMenu.getCurrentMasteryLevel();
                        int masteryRequired = ModEntry.Config.MasteryRequiredForCave;
                        int totalSkills = Game1.player.farmingLevel.Value / 10 + Game1.player.fishingLevel.Value / 10 + Game1.player.foragingLevel.Value / 10 + Game1.player.miningLevel.Value / 10 + Game1.player.combatLevel.Value / 10;
                        if (masteryLevel >= masteryRequired || totalSkills >= 5)
                        {
                            Game1.playSound("doorClose");
                            Game1.warpFarmer("MasteryCave", 7, 11, 0);
                        }
                        else
                        {
                            Game1.drawObjectDialogue(new List<string> {
                                Game1.content.LoadString("Strings\\1_6_Strings:MasteryCave", totalSkills).Replace(".",""),
                                ModEntry.ModHelper.Translation.Get("transcend-mortal-knowledge") + $" ({masteryLevel}/{masteryRequired})"
                            });
                        }
                        __result = true;
                    }
                    if (actionType == "MasteryCave_Pedestal")
                    {
                        Game1.activeClickableMenu = new MasteryTrackerMenu();
                        __result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogMonitor.Log($"Failed in {nameof(performActionPostfix)}:\n{ex}", LogLevel.Error);
            }
        }

        // Mostrar bien cuál pilar se puede obtener.
        internal static void MakeMapModificationsPostfix(GameLocation __instance)
        {
            try
            {
                if (__instance.Name == "MasteryCave")
                {
                    for (int which = 0; which < 5; which++)
                    {
                        bool enoughProfessions = MasterySkillsPage.skills.Find(s => s.Id == which)!.unlockedProfessions() >= (ModEntry.Config.ExtraRequiredProfession ? 3 : 2);

                        Game1.stats.Get("MasteryExp");
                        bool freeLevel = MasteryTrackerMenu.getCurrentMasteryLevel() > (int)Game1.stats.Get("masteryLevelsSpent");

                        if (!enoughProfessions || !freeLevel)
                        {
                            Game1.currentLocation.removeTemporarySpritesWithID(8765 + which);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Fication");
                LogMonitor.Log($"Failed in {nameof(MakeMapModificationsPostfix)}:\n{ex}", LogLevel.Error);
            }
        }
        
        
       // Profession Forget After Recount Used Mastery Levels
        internal static void answerDialogueActionPostFix(GameLocation __instance, string questionAndAnswer)
        {
            if (!questionAndAnswer.StartsWith("professionForget_")) return;
            
            ModEntry.recountUsedMasteryLevels();
        }
    }
}
